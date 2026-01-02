using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    // --- BİLEŞEN REFERANSLARI ---
    PlayerManager playerManager;
    InputHandler inputHandler;
    public Rigidbody playerRigidbody; // Fizik motoru bileşeni
    AnimatorHandler animatorHandler;
    Transform cameraObject; // Ana kamera referansı

    [Header("Hareket Hız Ayarları")]
    [Tooltip("Karakterin normal yürüme hızı.")]
    public float movementSpeed = 5;
    [Tooltip("Karakterin koşma (Sprint) hızı.")]
    public float sprintSpeed = 7;
    [Tooltip("Karakterin olduğu yerde dönme hızı.")]
    public float rotationSpeed = 10;

    [Tooltip("Yuvarlanma anında karaktere uygulanan anlık kuvvet (Fırlatma gücü).")]
    public float rollingVelocity = 25f; 

    [Header("Düşme ve Zıplama Ayarları")]
    [Tooltip("Karakterin havada kaldığı süreyi tutar.")]
    public float inAirTimer;
    [Tooltip("Havadayken ileriye doğru minimal hareket hızı.")]
    public float leapingVelocity = 3f;
    [Tooltip("Yere düşüş hızı (Yerçekimi ivmesi).")]
    public float fallingVelocity = 33f;
    [Tooltip("Zemin kontrolü için ışın (Raycast) başlangıç yüksekliği.")]
    public float rayCastHeightOffset = 0.5f;
    [Tooltip("Hangi objelerin 'Zemin' sayılacağı.")]
    public LayerMask groundLayer;

    [Header("Durum Bilgileri")]
    [Tooltip("Karakter şu an koşuyor mu?")]
    public bool isSprinting;
    [Tooltip("Karakter zemine basıyor mu?")]
    public bool isGrounded;

    // Hareket yönünü tutan vektör
    Vector3 moveDirection;

    void Start()
    {
        // Bileşenleri al
        playerManager = GetComponent<PlayerManager>();
        playerRigidbody = GetComponent<Rigidbody>();
        inputHandler = GetComponent<InputHandler>();
        animatorHandler = GetComponentInChildren<AnimatorHandler>();
        cameraObject = Camera.main.transform; // Sahnedeki Ana Kamerayı bul

        isGrounded = true; // Oyuna yerde başlıyoruz varsayalım

        if (groundLayer == 0)
        {
            groundLayer = 1; // Default layer
        }
    }

    /// <summary>
    /// Yuvarlanma ve Koşma mantığını yönetir.
    /// </summary>
    public void HandleRollingAndSprinting(float delta)
    {
        // Eğer zaten bir etkileşim (animasyon) içindeysek (örn: hasar alma), yuvarlanamayız.
        // Input'u iptal et ve çık.
        if (animatorHandler.anim.GetBool(AnimatorHandler.isInteractingHash))
        {
            inputHandler.rollFlag = false; 
            return;
        }

        // --- YUVARLANMA (ROLL) ---
        if (inputHandler.rollFlag)
        {
            // Yuvarlanma yönünü kameraya göre hesapla
            moveDirection = cameraObject.forward * inputHandler.vertical;
            moveDirection += cameraObject.right * inputHandler.horizontal;

            // Eğer bir yöne basılıyorsa (Hareket halindeyken yuvarlanma)
            if (inputHandler.moveAmount > 0)
            {
                animatorHandler.PlayTargetAnimation("Rolling", true); // Takla animasyonu
                moveDirection.y = 0; // Yere paralel olsun
                
                // Karakteri hareket yönüne döndür
                Quaternion rollRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = rollRotation;
                
                // İleri doğru fırlat
                playerRigidbody.AddForce(moveDirection.normalized * rollingVelocity, ForceMode.Impulse);
            }
            else
            {
                // Olduğu yerde yuvarlanma (Backstep veya Geri Kaçış)
                
                // Eğer kilitlendiğimiz bir hedef varsa
                if (CameraHandler.singleton.currentLockOnTarget != null)
                {
                    // Hedeften UZAKLAŞACAK yönü bul
                    Vector3 dirToTarget = transform.position - CameraHandler.singleton.currentLockOnTarget.position;
                    dirToTarget.y = 0;
                    dirToTarget.Normalize();
                    
                    // Arkamızı dönmeden geri kaçmak için rotasyonu ayarla
                    transform.rotation = Quaternion.LookRotation(dirToTarget);
                }
                else
                {
                    // Hedef yoksa, klasik "Geriye dön" mantığı
                    transform.rotation = Quaternion.LookRotation(-transform.forward);
                }

                animatorHandler.PlayTargetAnimation("Rolling", true); 
                
                // Karakterin önüne doğru (ki yukarıda ayarladık, kaçış yönü) fırlat
                playerRigidbody.AddForce(transform.forward * rollingVelocity, ForceMode.Impulse);
            }
            
            inputHandler.rollFlag = false; // İşlem tamam, bayrağı indir
        }

        // --- KİLİTLENME VE KOŞMA İLİŞKİSİ ---
        // Eğer kilitliysek, sadece ileri doğru koşabiliriz. Geri geri koşulmaz.
        if (CameraHandler.singleton.currentLockOnTarget != null)
        {
            if (inputHandler.vertical > 0f)
            {
                 // İleri basılıyor, koşmaya izin ver
            }
            else
            {
                isSprinting = false; // Geri veya yan basılıyorsa koşma
                return;
            }
        }

        // --- KOŞMA (SPRINT) ---
        // Sprint tuşuna basılı mı VE hareket ediyor muyuz?
        if (inputHandler.sprintFlag && inputHandler.moveAmount > 0.5f)
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }
    }

    /// <summary>
    /// Tüm hareket fonksiyonlarını sırayla çağıran ana yönetici.
    /// </summary>
    public void HandleAllMovement(float delta)
    {
        // Önce düşüyor muyuz kontrol et
        HandleFalling(delta, moveDirection);

        // Eğer yuvarlanıyorsak veya animasyondaysak normal yürümeyi iptal et (Fizik çakışmasını önle)
        if (inputHandler.rollFlag || animatorHandler.anim.GetBool(AnimatorHandler.isInteractingHash))
            return;

        // Yürüme ve Dönme işlemlerini yap
        HandleMovement(delta);
        HandleRotation(delta);
    }

    /// <summary>
    /// Karakterin yürüme/koşma hızını ve vektörünü hesaplar.
    /// </summary>
    private void HandleMovement(float delta)
    {
        // Blok yaparken veya ölüyken hareket edemezsin
        if (playerManager.isBlocking || playerManager.isDead)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            return;
        }

        // Yuvarlanırken yön değiştirilemez
        if (inputHandler.rollFlag)
            return;

        // Havadayken yön değiştirilemez (Opsiyonel: Havada kontrol istenirse burası değişir)
        if (isGrounded == false)
            return;

        // Kamera yönüne göre hareket vektörü
        moveDirection = cameraObject.forward * inputHandler.vertical;
        moveDirection += cameraObject.right * inputHandler.horizontal;

        moveDirection.Normalize();
        moveDirection.y = 0; // Yere yapışık kal

        // Hız belirleme
        float speed = movementSpeed;
        if (isSprinting)
        {
            speed = sprintSpeed;
        }

        // RAGE MODE: Öfke aktifse %30 daha hızlı koş
        PlayerRageManager rageManager = GetComponent<PlayerRageManager>();
        if (rageManager != null && rageManager.isRageActive)
        {
            speed *= 1.3f; 
        }
        moveDirection *= speed;

        // Fizik motoruna hızı uygula (Y düşüş hızını koruyarak)
        Vector3 movementVelocity = moveDirection;
        playerRigidbody.linearVelocity = new Vector3(movementVelocity.x, playerRigidbody.linearVelocity.y, movementVelocity.z);
    }

    /// <summary>
    /// Karakterin dönme (rotasyon) işlemlerini yönetir.
    /// </summary>
    public void HandleRotation(float delta)
    {
        if (playerManager.isBlocking || playerManager.isDead) return;
        if (animatorHandler.anim.GetBool(AnimatorHandler.isInteractingHash)) return;

        // A. KİLİTLENME VARSA (LOCK-ON)
        if (CameraHandler.singleton.currentLockOnTarget != null)
        {
            // Gövdeyi hedefe doğru döndür
            Vector3 rotationDirection = CameraHandler.singleton.currentLockOnTarget.position - transform.position;
            rotationDirection.y = 0;
            rotationDirection.Normalize();
            
            Quaternion tr = Quaternion.LookRotation(rotationDirection);
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, rotationSpeed * delta);
            transform.rotation = targetRotation;
            return;
        }

        // B. NORMAL DÖNÜŞ (SERBEST KAMERA)
        Vector3 targetDir = Vector3.zero;
        
        targetDir = cameraObject.forward * inputHandler.vertical;
        targetDir += cameraObject.right * inputHandler.horizontal;

        targetDir.Normalize();
        targetDir.y = 0;

        // Hareket yoksa karakter olduğu yöne bakmaya devam etsin (Sıfıra dönmesin)
        if (targetDir == Vector3.zero)
            targetDir = transform.forward;

        // Yumuşak dönüş (Slerp)
        Quaternion tr_normal = Quaternion.LookRotation(targetDir);
        Quaternion targetRotation_normal = Quaternion.Slerp(transform.rotation, tr_normal, rotationSpeed * delta);

        transform.rotation = targetRotation_normal;
    }

    /// <summary>
    /// Yer çekimi ve düşme kontrolü.
    /// </summary>
    public void HandleFalling(float delta, Vector3 moveDirection)
    {
        isGrounded = false;
        RaycastHit hit;
        
        // Karakterin biraz yukarısından aşağı doğru ışın (Ray) atıyoruz
        Vector3 origin = transform.position;
        origin.y += rayCastHeightOffset;

        // SphereCast: Kalın bir ışın (Küre) atarak zemini daha iyi algılar
        if (Physics.SphereCast(origin, 0.2f, Vector3.down, out hit, 1f, groundLayer))
        {
            isGrounded = true;
        }

        if (!isGrounded)
        {
            // HAVADA
            animatorHandler.anim.SetBool(AnimatorHandler.isGroundedHash, false);
            animatorHandler.PlayTargetAnimation("Falling", true); // Düşme animasyonu

            inAirTimer += delta;
            
            // Havadayken hafifçe ileri süzülme (Duvar kenarında takılmayı önler)
            playerRigidbody.AddForce(transform.forward * leapingVelocity);
            
            // Yerçekimini uygula (Zamanla hızlanan düşüş)
            Vector3 vel = playerRigidbody.linearVelocity;
            vel.y -= fallingVelocity * delta;
            playerRigidbody.linearVelocity = vel;
        }
        else
        {
            // YERDE
            animatorHandler.anim.SetBool(AnimatorHandler.isGroundedHash, true);
            inAirTimer = 0;
            
            // Eğer havadan yeni indiysek "Land" (İniş) animasyonu çal
            if (isGrounded && animatorHandler.anim.GetBool(AnimatorHandler.isGroundedHash) == false)
            {
                animatorHandler.PlayTargetAnimation("Land", true);
            }
        }
    }

    /// <summary>
    /// Dışarıdan darbe alınca karakteri itmek için kullanılır.
    /// </summary>
    public void ApplyKnockback(Vector3 direction, float force)
    {
        direction.Normalize();
        direction.y = 0; 
        
        // Eğer yön yoksa geriye doğru it
        if (direction == Vector3.zero)
            direction = -transform.forward;

        // Mevcut hızı sıfırla (Darbe net hissedilsin)
        playerRigidbody.linearVelocity = new Vector3(0, playerRigidbody.linearVelocity.y, 0);

        // Anlık kuvvet uygula
        playerRigidbody.AddForce(direction * force, ForceMode.Impulse);
    }
}
