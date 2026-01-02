using UnityEngine;

public class AnimatorHandler : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Karakterin Animator bileşeni.")]
    public Animator anim;
    
    // Gerekli diğer bileşenler
    InputHandler inputHandler;
    PlayerLocomotion playerLocomotion;
    PlayerManager playerManager;
    
    // --- OPTIMIZATION (HASH IDs) ---
    // String karşılaştırması yerine Integer ID kullanarak performansı artırıyoruz.
    public static readonly int isInteractingHash = Animator.StringToHash("isInteracting");
    public static readonly int isBlockingHash = Animator.StringToHash("isBlocking");
    public static readonly int isGroundedHash = Animator.StringToHash("isGrounded");
    public static readonly int isLockedOnHash = Animator.StringToHash("IsLockedOn");
    public static readonly int inputXHash = Animator.StringToHash("InputX");
    public static readonly int inputYHash = Animator.StringToHash("InputY");
    public static readonly int moveAmountHash = Animator.StringToHash("MoveAmount");
    // --------------------------------

    [Header("Animasyon Ayarları")]
    [Tooltip("Komboların yapılıp yapılamayacağını kontrol eder.")]
    public bool canDoCombo;
    [Tooltip("Yuvarlanma hızı (Animasyon kök hareketi yerine manuel hız).")]
    public float rollSpeed = 4f;

    void Awake()
    {
        anim = GetComponent<Animator>();
        // Parent objeden diğer scriptleri bul
        inputHandler = GetComponentInParent<InputHandler>();
        playerLocomotion = GetComponentInParent<PlayerLocomotion>();
        playerManager = GetComponentInParent<PlayerManager>();
    }

    /// <summary>
    /// Animator parametrelerini günceller (Yürüme, Koşma, Kilitlenme vb.)
    /// </summary>
    public void UpdateAnimatorValues(float moveAmount, float horizontalMovement, float verticalMovement, bool isLockedOn)
    {
        if (playerManager.isDead) return;

        // Kilitlenme durumuna göre animasyon blend tree ayarları
        if (isLockedOn)
        {
            anim.SetBool(isLockedOnHash, true);

            // Değerleri yuvarlayarak (Snapping) daha kesin animasyon geçişleri sağla
            float snappedHorizontal;
            float snappedVertical;

            #region Horizontal Snapping
            if (horizontalMovement > 0 && horizontalMovement < 0.55f) snappedHorizontal = 0.5f;
            else if (horizontalMovement > 0.55f) snappedHorizontal = 1;
            else if (horizontalMovement < 0 && horizontalMovement > -0.55f) snappedHorizontal = -0.5f;
            else if (horizontalMovement < -0.55f) snappedHorizontal = -1;
            else snappedHorizontal = 0;
            #endregion

            #region Vertical Snapping
            if (verticalMovement > 0 && verticalMovement < 0.55f) snappedVertical = 0.5f;
            else if (verticalMovement > 0.55f) snappedVertical = 1;
            else if (verticalMovement < 0 && verticalMovement > -0.55f) snappedVertical = -0.5f;
            else if (verticalMovement < -0.55f) snappedVertical = -1;
            else snappedVertical = 0;
            #endregion

            // Koşuyorsa değerleri maksimize et
            if (playerLocomotion.isSprinting)
            {
                snappedHorizontal = horizontalMovement; 
                snappedVertical = 2; 
            }

            anim.SetFloat(inputXHash, snappedHorizontal, 0.1f, Time.deltaTime);
            anim.SetFloat(inputYHash, snappedVertical, 0.1f, Time.deltaTime);
        }
        else
        {
            // Kilit yoksa sadece hareket büyüklüğüne göre animasyon oynat
            anim.SetBool(isLockedOnHash, false);
            anim.SetFloat(moveAmountHash, moveAmount, 0.1f, Time.deltaTime);
        }
    }

    /// <summary>
    /// Belirli bir animasyonu oynatır (Saldırı, Yuvarlanma vb.)
    /// isInteracting = true ise oyuncunun kontrolünü kısıtlar.
    /// </summary>
    public void PlayTargetAnimation(string targetAnim, bool isInteracting)
    {
        anim.applyRootMotion = isInteracting; 

        anim.SetBool(isInteractingHash, isInteracting);
        anim.CrossFade(targetAnim, 0.2f);
    }
    
    // --- ANIMASYON EVENTLERİ ("Animation Event" ile çağrılır) ---

    // Kombo penceresini açar
    public void EnableCombo()
    {
        canDoCombo = true;
    }

    // Kombo penceresini kapatır
    public void DisableCombo()
    {
        canDoCombo = false;
    }

    // Kök hareketini (Root Motion) işler
    public void OnAnimatorMove()
    {
        if (playerManager.isInteracting == false)
            return;

        float delta = Time.deltaTime;
        if (delta <= 0) return;

        Rigidbody rb = GetComponentInParent<Rigidbody>(); 

        if (rb != null)
        {
            rb.linearDamping = 0;
            Vector3 velocity;

            // Yuvarlanma sırasında özel hız uygula
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Rolling"))
            {
                velocity = transform.forward * rollSpeed;
                velocity.y = rb.linearVelocity.y; 
            }
            else
            {
                // Diğer durumlarda animasyonun kendi hareketini kullan
                velocity = anim.deltaPosition / delta;
                velocity.y = rb.linearVelocity.y; 
            }
            
            rb.linearVelocity = velocity; 
        }
    }
}