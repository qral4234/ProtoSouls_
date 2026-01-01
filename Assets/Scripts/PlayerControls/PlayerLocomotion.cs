using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    PlayerManager playerManager;
    InputHandler inputHandler;
    public Rigidbody playerRigidbody;
    AnimatorHandler animatorHandler;
    Transform cameraObject;

    [Header("Hareket Ayarları")]
    [Tooltip("Karakterin normal yürüme hızı.")]
    public float movementSpeed = 5;
    [Tooltip("Karakterin koşma (Sprint) hızı.")]
    public float sprintSpeed = 7;
    [Tooltip("Karakterin dönme hızı.")]
    public float rotationSpeed = 10;

    [Tooltip("Yuvarlanma sırasında karaktere uygulanan anlık fırlatma gücü.")]
    public float rollingVelocity = 25f; 

    [Header("Düşme ve İniş Ayarları")]
    [Tooltip("Karakterin havada kaldığı süre.")]
    public float inAirTimer;
    [Tooltip("Zıplama veya düşerken ileri doğru uygulanan ekstra hız.")]
    public float leapingVelocity = 3f;
    [Tooltip("Düşüş hızı (Yerçekimi etkisi).")]
    public float fallingVelocity = 33f;
    [Tooltip("Zemin kontrolü için raycast'in başlangıç yüksekliği.")]
    public float rayCastHeightOffset = 0.5f;
    [Tooltip("Zemin (Yürünebilir alan) layerları.")]
    public LayerMask groundLayer;

    [Header("Hareket Durumları")]
    [Tooltip("Karakter şu an koşuyor mu?")]
    public bool isSprinting;
    [Tooltip("Karakter yerde mi?")]
    public bool isGrounded;

    Vector3 moveDirection;

    void Start()
    {
        playerManager = GetComponent<PlayerManager>();
        playerRigidbody = GetComponent<Rigidbody>();
        inputHandler = GetComponent<InputHandler>();
        animatorHandler = GetComponentInChildren<AnimatorHandler>();
        cameraObject = Camera.main.transform;

        isGrounded = true;

        if (groundLayer == 0)
        {
            groundLayer = 1; 
        }
    }

    public void HandleRollingAndSprinting(float delta)
    {
        if (animatorHandler.anim.GetBool("isInteracting"))
        {
            // FIX: Eğer animasyon sırasındaysak ve tuşa basıldıysa, o girdiyi yut (iptal et).
            // Yoksa animasyon bitince "takılı kalmış" gibi tekrar takla atar.
            inputHandler.rollFlag = false; 
            return;
        }

        // --- YUVARLANMA (ROLL) MANTIĞI ---
        if (inputHandler.rollFlag)
        {
            moveDirection = cameraObject.forward * inputHandler.vertical;
            moveDirection += cameraObject.right * inputHandler.horizontal;

            if (inputHandler.moveAmount > 0)
            {
                animatorHandler.PlayTargetAnimation("Rolling", true);
                moveDirection.y = 0;
                
                // Hangi modda olursak olalım (Lock-On dahil), karakter hareket ettiği yöne dönsün.
                // Böylece S'ye basınca arkasını dönüp kaçar (İleri takla animasyonuyla).
                Quaternion rollRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = rollRotation;
                
                playerRigidbody.AddForce(moveDirection.normalized * rollingVelocity, ForceMode.Impulse);
            }
            else
            {
                // HİÇBİR YERE BASILMIYORSA (Olduğu yerde Shift)
                
                // Karakteri tam arkaya döndür (180 derece)
                transform.rotation = Quaternion.LookRotation(-transform.forward);
                
                // Şimdi öne doğru (yani eskiden arka olan tarafa) takla at
                animatorHandler.PlayTargetAnimation("Rolling", true); 
                
                // Kuvveti yeni baktığı yöne (uzaklaşacak şekilde) uygula
                playerRigidbody.AddForce(transform.forward * rollingVelocity, ForceMode.Impulse);
            }
            
            inputHandler.rollFlag = false;
        }

        // --- KİLİTLENME (LOCK-ON) KONTROLÜ ---
        if (CameraHandler.singleton.currentLockOnTarget != null)
        {
            // Sadece ileri (W) basılıysa koşmaya izin ver
            if (inputHandler.vertical > 0f)
            {
                 // Devam et, aşağıdaki kod isSprinting'i açacak
            }
            else
            {
                isSprinting = false;
                return;
            }
        }

        // --- KOŞMA (SPRINT) MANTIĞI ---
        if (inputHandler.sprintFlag && inputHandler.moveAmount > 0.5f)
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }
    }

    public void HandleAllMovement(float delta)
    {
        HandleFalling(delta, moveDirection);

        if (inputHandler.rollFlag || animatorHandler.anim.GetBool("isInteracting"))
            return;

        HandleMovement(delta);
        HandleRotation(delta);
    }

    private void HandleMovement(float delta)
    {
        if (playerManager.isBlocking)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            return;
        }

        if (playerManager.isDead)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            return;
        }

        if (inputHandler.rollFlag)
            return;

        if (isGrounded == false)
            return;

        // Kamera yönüne göre hareket vektörünü hesapla
        moveDirection = cameraObject.forward * inputHandler.vertical;
        moveDirection += cameraObject.right * inputHandler.horizontal;

        moveDirection.Normalize();
        moveDirection.y = 0;

        float speed = movementSpeed;
        if (isSprinting)
        {
            speed = sprintSpeed;
        }

        // RAGE MODE ENTEGRASYONU
        PlayerRageManager rageManager = GetComponent<PlayerRageManager>();
        if (rageManager != null && rageManager.isRageActive)
        {
            speed *= 1.3f; // Öfke modunda %30 hız artışı
        }
        moveDirection *= speed;

        Vector3 movementVelocity = moveDirection;
        playerRigidbody.linearVelocity = new Vector3(movementVelocity.x, playerRigidbody.linearVelocity.y, movementVelocity.z);
    }

    public void HandleRotation(float delta)
    {
        if (playerManager.isBlocking)
            return;

        if (playerManager.isDead)
            return;
            
        if (animatorHandler.anim.GetBool("isInteracting"))
            return;

        // KİLİTLENME (LOCK-ON) VARSA
        if (CameraHandler.singleton.currentLockOnTarget != null)
        {
            Vector3 rotationDirection = CameraHandler.singleton.currentLockOnTarget.position - transform.position;
            rotationDirection.y = 0;
            rotationDirection.Normalize();
            Quaternion tr = Quaternion.LookRotation(rotationDirection);
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, rotationSpeed * delta);
            transform.rotation = targetRotation;
            return;
        }

        // NORMAL ROTASYON
        Vector3 targetDir = Vector3.zero;
        float moveOverride = inputHandler.moveAmount;

        targetDir = cameraObject.forward * inputHandler.vertical;
        targetDir += cameraObject.right * inputHandler.horizontal;

        targetDir.Normalize();
        targetDir.y = 0;

        if (targetDir == Vector3.zero)
            targetDir = transform.forward;

        float rs = rotationSpeed;

        Quaternion tr_normal = Quaternion.LookRotation(targetDir);
        Quaternion targetRotation_normal = Quaternion.Slerp(transform.rotation, tr_normal, rs * delta);

        transform.rotation = targetRotation_normal;
    }

    public void HandleFalling(float delta, Vector3 moveDirection)
    {
        isGrounded = false;
        RaycastHit hit;
        Vector3 origin = transform.position;
        origin.y += rayCastHeightOffset;

        // Yer kontrolü yap (Raycast)
        if (Physics.SphereCast(origin, 0.2f, Vector3.down, out hit, 1f, groundLayer))
        {
            isGrounded = true;
        }

        if (!isGrounded)
        {
            animatorHandler.anim.SetBool("isGrounded", false);
            animatorHandler.PlayTargetAnimation("Falling", true);

            inAirTimer += delta;
            
            // Havadayken ileri doğru hafif itme
            playerRigidbody.AddForce(transform.forward * leapingVelocity);
            
            // Düşme hızını arttır
            Vector3 vel = playerRigidbody.linearVelocity;
            vel.y -= fallingVelocity * delta;
            playerRigidbody.linearVelocity = vel;
        }
        else
        {
            animatorHandler.anim.SetBool("isGrounded", true);
            inAirTimer = 0;
            
            // Yere yeni indiyse "Land" animasyonunu çal
            if (isGrounded && animatorHandler.anim.GetBool("isGrounded") == false)
            {
                animatorHandler.PlayTargetAnimation("Land", true);
            }
        }
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        direction.Normalize();
        direction.y = 0; 
        
        if (direction == Vector3.zero)
            direction = -transform.forward;

        playerRigidbody.linearVelocity = new Vector3(0, playerRigidbody.linearVelocity.y, 0);

        playerRigidbody.AddForce(direction * force, ForceMode.Impulse);
    }
}
