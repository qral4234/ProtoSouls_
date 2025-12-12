using UnityEngine;

public class AnimatorHandler : MonoBehaviour
{
    public Animator anim;
    InputHandler inputHandler;
    PlayerLocomotion playerLocomotion;
    PlayerManager playerManager;

    void Awake()
    {
        anim = GetComponent<Animator>();
        inputHandler = GetComponentInParent<InputHandler>();
        playerLocomotion = GetComponentInParent<PlayerLocomotion>();
        playerManager = GetComponentInParent<PlayerManager>();
    }

    public void UpdateAnimatorValues(float moveAmount)
    {
        if (playerManager.isDead)
            return;

        // 0.1f Damping (Yumuşatma) ile değeri gönder
        anim.SetFloat("MoveAmount", moveAmount, 0.1f, Time.deltaTime);
    }

    public void PlayTargetAnimation(string targetAnim, bool isInteracting)
    {
        // BU SATIR EKLENDİ: Etkileşimdeyken (Takla atarken) Root Motion'ı aktif et
        anim.applyRootMotion = isInteracting; 

        anim.SetBool("isInteracting", isInteracting);
        anim.CrossFade(targetAnim, 0.2f);
    }
    
    public bool canDoCombo;

    public void EnableCombo()
    {
        canDoCombo = true;
    }

    public void DisableCombo()
    {
        canDoCombo = false;
    }

    public float rollSpeed = 4f;

    public void OnAnimatorMove()
    {
        // Eğer etkileşimde değilsek (Normal koşuyorsak) iptal et
        // Çünkü o zaman hareketi PlayerLocomotion yönetiyor
        if (playerManager.isInteracting == false)
            return;

        float delta = Time.deltaTime;
        
        // Rigidbody'ye en güvenli yoldan ulaşalım
        Rigidbody rb = GetComponent<Rigidbody>(); 

        if (rb != null)
        {
            // Etkileşim sırasında sürtünmeyi sıfırla
            rb.linearDamping = 0; // Unity 6 (Eski sürümse: drag = 0)
            Vector3 velocity;

            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Rolling"))
            {
                // Rolling için manuel hız
                velocity = transform.forward * rollSpeed;
                velocity.y = rb.linearVelocity.y; // Gravity koru
            }
            else
            {
                // Diğer animasyonlar (örn. Saldırı) için Root Motion kullan
                velocity = anim.deltaPosition / delta;
                velocity.y = rb.linearVelocity.y; // Gravity koru
            }
            
            // Hızı uygula
            rb.linearVelocity = velocity; // Unity 6 (Eski sürümse: velocity = velocity)
        }
    }
}