using UnityEngine;

public class PlayerStats : CharacterStats
{
    public int maxStamina = 400;
    public float currentStamina;

    public HealthBar healthBar;
    public StaminaBar staminaBar;

    [Header("Stamina Settings")]
    public float staminaRegenAmount = 15f;
    public float staminaRegenDelay = 2.0f;
    public float staminaRegenTimer = 0;

    private float regenMultiplier = 1.0f;

    PlayerManager playerManager;
    AnimatorHandler animatorHandler;

    public override void Start()
    {
        base.Start();
        playerManager = GetComponent<PlayerManager>();
        animatorHandler = GetComponentInChildren<AnimatorHandler>();

        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
        }

        currentStamina = maxStamina;
        if (staminaBar != null)
        {
            staminaBar.SetMaxStamina(maxStamina);
        }
    }

    public override void TakeDamage(int damage, string damageAnimation = "Damage")
    {
        if (playerManager.isBlocking && currentStamina > 0)
        {
            float staminaDamage = damage / 2; 
            TakeStaminaDamage(staminaDamage);
            animatorHandler.PlayTargetAnimation("BlockedImpact", true);
            Debug.Log("Blocked!");
            return;
        }

        currentHealth = currentHealth - damage;

        if (healthBar != null)
        {
            healthBar.SetCurrentHealth(currentHealth);
        }

        animatorHandler.PlayTargetAnimation(damageAnimation, true);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            HandleDeath();
        }
    }

    public void SetRegenMultiplier(float multiplier)
    {
        regenMultiplier = multiplier;
    }

    void Update()
    {
        HandleStaminaRegen();
    }

    private void HandleStaminaRegen()
    {
        if (currentStamina < maxStamina)
        {
            // Check if we have waited long enough to start regen
            if (staminaRegenTimer > staminaRegenDelay)
            {
                currentStamina += staminaRegenAmount * regenMultiplier * Time.deltaTime;

                if (currentStamina > maxStamina)
                {
                    currentStamina = maxStamina;
                }

                if (staminaBar != null)
                {
                    staminaBar.SetCurrentStamina(currentStamina);
                }
            }
            else
            {
                staminaRegenTimer += Time.deltaTime;
            }
        }
        else
        {
            staminaRegenTimer = 0;
        }
    }

    public void TakeStaminaDamage(float damage)
    {
        currentStamina = currentStamina - damage;
        staminaRegenTimer = 0;

        if (staminaBar != null)
        {
            staminaBar.SetCurrentStamina(currentStamina);
        }
    }

    public override void HandleDeath()
    {
        currentHealth = 0;
        playerManager.isDead = true;
        
        if (healthBar != null)
        {
            healthBar.SetCurrentHealth(0);
        }

        animatorHandler.PlayTargetAnimation("Death", true);

        // Disable physics to prevent pushing/interactions
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
    }
}
