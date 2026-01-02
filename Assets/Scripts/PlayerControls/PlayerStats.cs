using UnityEngine;
using UnityEngine.UI; 

public class PlayerStats : CharacterStats
{
    // --- TEMEL ÖZELLİKLER ---
    public int maxStamina = 400;
    public float currentStamina;

    // --- UI REFERANSLARI ---
    public HealthBar healthBar; // Can barı scripti
    public StaminaBar staminaBar; // Stamina barı scripti
    public Text potionCountText; // Ekranda kalan pot sayısını gösteren yazı

    [Header("Stamina Yenilenme Ayarları")]
    public float staminaRegenAmount = 15f; // Saniyede dolan miktar
    public float staminaRegenDelay = 2.0f; // Harcadıktan sonra ne kadar beklesin?
    public float staminaRegenTimer = 0;

    private float regenMultiplier = 1.0f; // Yenilenme hızı çarpanı (Yürürken yavaş, dururken hızlı)

    // --- BİLEŞEN REFERANSLARI ---
    PlayerManager playerManager;
    AnimatorHandler animatorHandler;
    PlayerHitFeedback playerHitFeedback; // Hasar alınca yanıp sönme efekti

    [Header("İyileşme (Pot) Ayarları")]
    public int healCount = 3;  // Kaç şişe iksir var?
    public int healAmount = 50; // Bir şişe ne kadar can verir?
    public HealingVisuals healingVisuals; // İyileşme görsel efekti
    

    [Header("Fiziksel Tepki Ayarları")]
    public float knockbackForce = 25f; // Hasar alınca ne kadar geriye itilecek?

    public override void Start()
    {
        base.Start(); // Parent (CharacterStats) Start'ını çalıştır
        
        // Bileşenleri bul
        playerManager = GetComponent<PlayerManager>();
        animatorHandler = GetComponentInChildren<AnimatorHandler>();
        playerHitFeedback = GetComponent<PlayerHitFeedback>();
        
        // Görsel efekt scriptini bul veya ekle
        healingVisuals = GetComponent<HealingVisuals>();
        if (healingVisuals == null)
        {
            healingVisuals = gameObject.AddComponent<HealingVisuals>();
        }

        // Barları başlat
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
        }

        currentStamina = maxStamina;
        if (staminaBar != null)
        {
            staminaBar.SetMaxStamina(maxStamina);
        }

        // UI'da pot sayısını yaz
        if(potionCountText != null)
        {
            potionCountText.text = healCount.ToString();
        }
    }

    /// <summary>
    /// Oyuncuyu iyileştirir (Pot içer).
    /// </summary>
    public void HealPlayer()
    {
        if (isDead) return;
        if (healCount <= 0) return; // İksir bittiyse içemez
        if (currentHealth >= maxHealth) return; // Can zaten full ise içemez

        // 1. Canı Yenile
        currentHealth += healAmount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        // 2. Hakkı Azalt
        healCount--;
        
        // 3. UI Güncelle
        if(potionCountText != null)
        {
             potionCountText.text = healCount.ToString();
        }

        if (healthBar != null)
        {
            healthBar.SetCurrentHealth(currentHealth);
        }

        // 4. Görsel Efekti Patlat
        if (healingVisuals != null)
        {
            healingVisuals.PlayHealingEffect(transform.position + Vector3.up * 1.0f); 
        }
    }

    /// <summary>
    /// Hasar alma fonksiyonu (CharacterStats'tan override edildi).
    /// </summary>
    public override void TakeDamage(int damage, float poiseDamage, float attackerKnockbackForce, string damageAnimation = "Damage", Transform damageSource = null, Vector3 hitPoint = default)
    {
        // Bloklama Kontrolü: Eğer blok yapıyorsak can yerine Stamina düşsün.
        if (playerManager.isBlocking && currentStamina > 0)
        {
            float staminaDamage = damage / 2; 
            TakeStaminaDamage(staminaDamage);
            animatorHandler.PlayTargetAnimation("BlockedImpact", true); // Bloklama animasyonu
            return; // Can düşmeden çık
        }

        // Normal Hasar
        currentHealth = currentHealth - damage;

        // Can barını güncelle
        if (healthBar != null)
        {
            healthBar.SetCurrentHealth(currentHealth);
        }

        // Hasar animasyonu ve efekti
        animatorHandler.PlayTargetAnimation(damageAnimation, true);

        if (playerHitFeedback != null)
        {
            playerHitFeedback.PlayHitFeedback(); // Kırmızı yanıp sönme
        }

        // Geri Tepme (Knockback) Uygula
        if (damageSource != null)
        {
            PlayerLocomotion locomotion = GetComponent<PlayerLocomotion>();
            if (locomotion != null)
            {
                Vector3 knockbackDir = transform.position - damageSource.position; // Düşmandan bize doğru vektör
                locomotion.ApplyKnockback(knockbackDir, attackerKnockbackForce);
            }
        }

        // Ölüm Kontrolü
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

    // Zamanla Stamina dolmasını sağlar
    private void HandleStaminaRegen()
    {
        if (currentStamina < maxStamina)
        {
            // Eğer bekleme süresi dolduysa stamina doldur
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
                // Bekleme süresini say
                staminaRegenTimer += Time.deltaTime;
            }
        }
        else
        {
            staminaRegenTimer = 0;
        }
    }

    // Stamina harcama fonksiyonu
    public void TakeStaminaDamage(float damage)
    {
        currentStamina = currentStamina - damage;
        staminaRegenTimer = 0; // Harcayınca timer sıfırlanır, hemen dolmaya başlamaz

        if (staminaBar != null)
        {
            staminaBar.SetCurrentStamina(currentStamina);
        }
    }

    /// <summary>
    /// Oyuncu öldüğünde yapılacaklar.
    /// </summary>
    public override void HandleDeath()
    {
        currentHealth = 0;
        playerManager.isDead = true;
        isDead = true; 
        
        if (healthBar != null)
        {
            healthBar.SetCurrentHealth(0);
        }

        animatorHandler.PlayTargetAnimation("Death", true); // Ölüm animasyonu

        // Fizikleri kapat (Yere düşsün ama itilemesin)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // Çarpışmaları kapat (İçinden geçilsin)
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        // KAYBETME EKRANINI AÇ
        if(GameUIManager.instance != null)
        {
            GameUIManager.instance.TriggerLoseGame();
        }
    }
}
