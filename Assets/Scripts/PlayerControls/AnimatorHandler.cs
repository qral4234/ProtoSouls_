using UnityEngine;

public class AnimatorHandler : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Karakterin Animator bileşeni.")]
    public Animator anim;
    
    InputHandler inputHandler;
    PlayerLocomotion playerLocomotion;
    PlayerManager playerManager;

    [Header("Animasyon Ayarları")]
    [Tooltip("Komboların yapılıp yapılamayacağını kontrol eder.")]
    public bool canDoCombo;
    [Tooltip("Yuvarlanma hızı (Animasyon kök hareketi yerine manuel hız).")]
    public float rollSpeed = 4f;

    void Awake()
    {
        anim = GetComponent<Animator>();
        inputHandler = GetComponentInParent<InputHandler>();
        playerLocomotion = GetComponentInParent<PlayerLocomotion>();
        playerManager = GetComponentInParent<PlayerManager>();
    }

    public void UpdateAnimatorValues(float moveAmount, float horizontalMovement, float verticalMovement, bool isLockedOn)
    {
        if (playerManager.isDead)
            return;

        if (isLockedOn)
        {
            // Kilitlenme aktifse: IsLockedOn parametresini aç ve Strafing değerlerini hesapla
            anim.SetBool("IsLockedOn", true);

            float snappedHorizontal;
            float snappedVertical;

            #region Horizontal Snapping (Yatay Değer Sabitleme)
            if (horizontalMovement > 0 && horizontalMovement < 0.55f) snappedHorizontal = 0.5f;
            else if (horizontalMovement > 0.55f) snappedHorizontal = 1;
            else if (horizontalMovement < 0 && horizontalMovement > -0.55f) snappedHorizontal = -0.5f;
            else if (horizontalMovement < -0.55f) snappedHorizontal = -1;
            else snappedHorizontal = 0;
            #endregion

            #region Vertical Snapping (Dikey Değer Sabitleme)
            if (verticalMovement > 0 && verticalMovement < 0.55f) snappedVertical = 0.5f;
            else if (verticalMovement > 0.55f) snappedVertical = 1;
            else if (verticalMovement < 0 && verticalMovement > -0.55f) snappedVertical = -0.5f;
            else if (verticalMovement < -0.55f) snappedVertical = -1;
            else snappedVertical = 0;
            #endregion

            if (playerLocomotion.isSprinting)
            {
                snappedHorizontal = horizontalMovement; 
                snappedVertical = 2; // Koşarken dikey değeri arttır
            }

            anim.SetFloat("InputX", snappedHorizontal, 0.1f, Time.deltaTime);
            anim.SetFloat("InputY", snappedVertical, 0.1f, Time.deltaTime);
        }
        else
        {
            // Kilitlenme yoksa: IsLockedOn kapat ve normal MoveAmount gönder
            anim.SetBool("IsLockedOn", false);
            anim.SetFloat("MoveAmount", moveAmount, 0.1f, Time.deltaTime);
        }
    }

    public void PlayTargetAnimation(string targetAnim, bool isInteracting)
    {
        // Animasyon oynatılırken Root Motion (animasyon bazlı hareket) kullanılsın mı?
        anim.applyRootMotion = isInteracting; 

        anim.SetBool("isInteracting", isInteracting);
        anim.CrossFade(targetAnim, 0.2f);
    }
    
    public void EnableCombo()
    {
        canDoCombo = true;
    }

    public void DisableCombo()
    {
        canDoCombo = false;
    }

    public void OnAnimatorMove()
    {
        if (playerManager.isInteracting == false)
            return;

        float delta = Time.deltaTime;
        Rigidbody rb = GetComponent<Rigidbody>(); 

        if (rb != null)
        {
            rb.linearDamping = 0;
            Vector3 velocity;

            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Rolling"))
            {
                // Rolling için manuel hız uygula
                velocity = transform.forward * rollSpeed;
                velocity.y = rb.linearVelocity.y; // Yerçekimini koru
            }
            else
            {
                // Diğer animasyonlar (örn. Saldırı) için Animator Root Motion kullan
                velocity = anim.deltaPosition / delta;
                velocity.y = rb.linearVelocity.y; // Yerçekimini koru
            }
            
            // Hesaplanan hızı karaktere uygula
            rb.linearVelocity = velocity; 
        }
    }
}