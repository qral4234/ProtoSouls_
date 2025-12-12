using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    InputHandler inputHandler;
    AnimatorHandler animatorHandler;
    PlayerLocomotion playerLocomotion;
    PlayerStats playerStats;
    PlayerAttacker playerAttacker;

    public bool isInteracting;
    public bool isBlocking;
    public bool isDead;

    [Header("Current Weapon")]
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

        inputHandler.TickInput(delta);

        // Blocking Logic
        if (inputHandler.blockingInput && !isInteracting) 
        {
            isBlocking = true;
        }
        else
        {
            isBlocking = false;
        }
        animatorHandler.anim.SetBool("isBlocking", isBlocking);

        // Stamina Regen Multiplier Check
        if (inputHandler.moveAmount > 0 && !inputHandler.sprintFlag)
        {
            playerStats.SetRegenMultiplier(2.0f);
        }
        else
        {
            playerStats.SetRegenMultiplier(1.0f);
        }

        // Sprinting Logic
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

        // Rolling Logic
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

        // Attacking Logic
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

        float moveAmount = inputHandler.moveAmount;
        if (playerLocomotion.isSprinting)
        {
            moveAmount = 2;
        }

        animatorHandler.UpdateAnimatorValues(moveAmount);
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
            CameraHandler.singleton.FollowTarget(delta);
            CameraHandler.singleton.HandleCameraRotation(delta, inputHandler.mouseX, inputHandler.mouseY);
            CameraHandler.singleton.HandleCameraCollisions(delta);
        }
    }
}