using UnityEngine;
using UnityEngine.UI; // Text için gerekli

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

    [Header("Heal Settings")]
    public int healCount = 3; 
    public int healAmount = 50; 
    public HealingVisuals healingVisuals; 
    public Text potionCountText; // Eklendi: Ekranda kaç şişe kaldığını yazacak

    [Header("Knockback Settings")]
    public float knockbackForce = 25f; 

    public override void Start()
    {
        base.Start();
        playerManager = GetComponent<PlayerManager>();
        animatorHandler = GetComponentInChildren<AnimatorHandler>();
        
        healingVisuals = GetComponent<HealingVisuals>();
        if (healingVisuals == null)
        {
            healingVisuals = gameObject.AddComponent<HealingVisuals>();
        }

        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
        }

        currentStamina = maxStamina;
        if (staminaBar != null)
        {
            staminaBar.SetMaxStamina(maxStamina);
        }

        // Başlangıçta UI'ı güncelle
        if(potionCountText != null)
        {
            potionCountText.text = healCount.ToString();
        }
    }

    public void HealPlayer()
    {
        if (isDead) return;
        if (healCount <= 0) return; 
        if (currentHealth >= maxHealth) return; 

        // 1. Canı Yenile
        currentHealth += healAmount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        // 2. Hakkı Azalt
        healCount--;
        Debug.Log("İyileşildi! Kalan Hak: " + healCount);
        
        // 3. UIText Güncelle (Eklendi)
        if(potionCountText != null)
        {
             potionCountText.text = healCount.ToString();
        }

        // 4. Bar Güncelle
        if (healthBar != null)
        {
            healthBar.SetCurrentHealth(currentHealth);
        }

        // 4. Görsel Efekti Oynat
        if (healingVisuals != null)
        {
            healingVisuals.PlayHealingEffect(transform.position + Vector3.up * 1.0f); // Gövde hizasında çıksın
        }

        // 5. Animasyon (Opsiyonel - Şimdilik sadece efekt)
        // animatorHandler.PlayTargetAnimation("Heal", true); 
    }

    public override void TakeDamage(int damage, float poiseDamage, float attackerKnockbackForce, string damageAnimation = "Damage", Transform damageSource = null)
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

        // KNOCKBACK LOGIC
        if (damageSource != null)
        {
            PlayerLocomotion locomotion = GetComponent<PlayerLocomotion>();
            if (locomotion != null)
            {
                // Direction: From Enemy -> To Player
                // If Enemy is at 0,0 and Player is at 0,2. Direction is (0,0,2) = Forward.
                Vector3 knockbackDir = transform.position - damageSource.position;
                locomotion.ApplyKnockback(knockbackDir, attackerKnockbackForce);
            }
        }

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
        isDead = true; // Base class değişkenini güncelle
        
        if (healthBar != null)
        {
            healthBar.SetCurrentHealth(0);
        }

        animatorHandler.PlayTargetAnimation("Death", true);

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
