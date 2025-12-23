using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    InputHandler inputHandler;
    AnimatorHandler animatorHandler;
    PlayerLocomotion playerLocomotion;
    PlayerStats playerStats;
    PlayerAttacker playerAttacker;

    [Header("Oyuncu Durumları")]
    [Tooltip("Karakter şu an bir animasyon etkileşiminde mi? (Saldırı, Yuvarlanma vb.)")]
    public bool isInteracting;
    [Tooltip("Karakter blok modunda mı?")]
    public bool isBlocking;
    [Tooltip("Karakter öldü mü?")]
    public bool isDead;

    [Header("Ekipman")]
    [Tooltip("Şu an kullanılan silah.")]
    public WeaponItem currentWeapon; 

    void Awake()
    {
        inputHandler = GetComponent<InputHandler>();
        playerLocomotion = GetComponent<PlayerLocomotion>();
        animatorHandler = GetComponent<AnimatorHandler>();
        playerStats = GetComponent<PlayerStats>();
        playerAttacker = GetComponent<PlayerAttacker>();
    }

    void Update()
    {
        if (isDead)
            return;

        float delta = Time.deltaTime;

        isInteracting = animatorHandler.anim.GetBool("isInteracting");

        // Girdileri dinle
        inputHandler.TickInput(delta);

        // Bloklama Mantığı
        if (inputHandler.blockingInput && !isInteracting) 
        {
            isBlocking = true;
        }
        else
        {
            isBlocking = false;
        }
        animatorHandler.anim.SetBool("isBlocking", isBlocking);

        // Stamina (Dayanıklılık) Yenilenme Hızı Kontrolü
        if (inputHandler.moveAmount > 0 && !inputHandler.sprintFlag)
        {
            playerStats.SetRegenMultiplier(2.0f); // Hareket ederken daha yavaş dolsun (örnek)
        }
        else
        {
            playerStats.SetRegenMultiplier(1.0f);
        }

        // Koşma (Sprint) Mantığı
        if (inputHandler.sprintFlag)
        {
            if (isInteracting)
                return;

            if (playerStats.currentStamina > 0)
            {
                playerStats.TakeStaminaDamage(10 * delta);
            }
            else
            {
                inputHandler.sprintFlag = false;
            }
        }

        // Yuvarlanma (Roll) Mantığı
        if (inputHandler.rollFlag)
        {
            if (isInteracting)
                return;

            if (playerStats.currentStamina > 0)
            {
                playerStats.TakeStaminaDamage(15);
            }
            else
            {
                inputHandler.rollFlag = false;
            }
        }

        playerLocomotion.HandleRollingAndSprinting(delta);

        // Saldırı Mantığı
        if (inputHandler.rb_Input)
        {
            if (playerStats.currentStamina > 0)
            {
                if (animatorHandler.canDoCombo)
                {
                    playerStats.TakeStaminaDamage(10);
                    playerAttacker.HandleCombo(currentWeapon);
                }
                else if (!isInteracting)
                {
                    if (currentWeapon != null)
                    {
                        playerStats.TakeStaminaDamage(10);
                        playerAttacker.HandleLightAttack(currentWeapon);
                    }
                }
            }

            inputHandler.rb_Input = false;
        }

        // Animasyon Değerlerini Güncelle
        float moveAmount = inputHandler.moveAmount;
        if (playerLocomotion.isSprinting)
        {
            moveAmount = 2; // Koşma animasyonu için
        }

        bool isLockedOn = CameraHandler.singleton.currentLockOnTarget != null;
        animatorHandler.UpdateAnimatorValues(moveAmount, inputHandler.horizontal, inputHandler.vertical, isLockedOn);
    }

    void FixedUpdate()
    {
        float delta = Time.fixedDeltaTime;

        if (playerLocomotion != null)
        {
            playerLocomotion.HandleAllMovement(delta);
        }
    }

    void LateUpdate()
    {
        float delta = Time.deltaTime;

        if (CameraHandler.singleton != null)
        {
            CameraHandler.singleton.HandleLockOn();
            CameraHandler.singleton.FollowTarget(delta);
            CameraHandler.singleton.HandleCameraRotation(delta, inputHandler.mouseX, inputHandler.mouseY);
            CameraHandler.singleton.HandleCameraCollisions(delta);
        }
    }
}