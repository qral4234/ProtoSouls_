using UnityEngine;

public class EnemyStats : CharacterStats
{
    Animator animator;
    EnemyLocomotionManager enemyLocomotionManager;
    EnemyManager enemyManager; 

    // Geri bildirimler (Hit Feedback)
    EnemyHitFeedback hitFeedback;
    BloodExplosionEffect bloodExplosionEffect;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        enemyLocomotionManager = GetComponent<EnemyLocomotionManager>();
        enemyManager = GetComponent<EnemyManager>(); 

        hitFeedback = GetComponent<EnemyHitFeedback>();
        bloodExplosionEffect = GetComponent<BloodExplosionEffect>();
        if(bloodExplosionEffect == null)
            bloodExplosionEffect = gameObject.AddComponent<BloodExplosionEffect>(); 
    }

    public BossHealthBar bossHealthBar; // Eğer boss ise can barı

    public override void Start()
    {
        base.Start();
        if (bossHealthBar != null)
        {
            bossHealthBar.SetMaxHealth(maxHealth);
        }
    }

    [Header("Combat Reaction (Savaş Tepkileri)")]
    public int hitCount = 0; // Kaç kere vuruldu? (Kombo kırmak için)
    public GameObject shockwavePrefab; // Geri itme efekti

    // Hasar Alma Fonksiyonu (CharacterStats'tan override)
    public override void TakeDamage(int damage, float poiseDamage, float knockbackForce, string damageAnimation = "Damage", Transform damageSource = null, Vector3 hitPoint = default)
    {
        currentHealth -= damage;

        if (bossHealthBar != null)
        {
            bossHealthBar.SetCurrentHealth(currentHealth);
        }

        // Hasar alınca bekleme süresini sıfırlama (Agresifleşsin mi?)
        if (enemyManager != null)
        {
            enemyManager.currentRecoveryTime += 0f; 
        }

        // Ölüm Kontrolü
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            HandleDeath(); 
            return;
        }

        // --- HİSSEDİLEBİLİRLİK (JUICE) ---
        hitCount++;

        // Her 2 vuruşta bir oyuncuyu geri it (Boss mekaniği)
        if (hitCount >= 2)
        {
            hitCount = 0; 
            CreateShockwaveEffect();
            PushPlayerBack();
        }
        else
        {
            // Normal hasar tepkisi (Parlamak vb.)
            if (hitFeedback != null)
            {
                hitFeedback.PlayHitFeedback();
            }
        }
    }

    // Kırmızı şok dalgası yarat
    private void CreateShockwaveEffect()
    {
        if (shockwavePrefab == null)
        {
            // Prefab yoksa kodla geçici oluştur (Backup)
            GameObject go = new GameObject("RedShockwave");
            go.transform.position = transform.position + Vector3.up * 0.5f; 
            go.AddComponent<RedShockwaveVisuals>(); // Eğer varsa
        }
        else
        {
            Instantiate(shockwavePrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }
    }

    // Oyuncuyu it (Alan hasarı olmadan sadece fiziksel itme)
    private void PushPlayerBack()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f);

        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Player"))
            {
                PlayerLocomotion playerLocomotion = col.GetComponent<PlayerLocomotion>();
                if (playerLocomotion != null)
                {
                    Vector3 pushDir = (col.transform.position - transform.position).normalized;
                    pushDir.y = 0.2f; // Hafif yukarı kaldır

                    playerLocomotion.ApplyKnockback(pushDir, 200f);
                }
            }
        }
    }

    // Ölüm Fonksiyonu
    public override void HandleDeath()
    {
        currentHealth = 0;
        if (animator != null)
        {
            isDead = true; 
            animator.SetBool("isDead", true); // Ölüm animasyonu
        }

        // Fizikleri kapat
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // AI'yı kapat
        if(enemyLocomotionManager != null) enemyLocomotionManager.enabled = false;
        if(enemyManager != null) enemyManager.enabled = false;
        
        // Kan efekti patlat
        if(bloodExplosionEffect != null)
        {
            bloodExplosionEffect.Explode();
        }

        // Oyunu Kazandın!
        if(GameUIManager.instance != null)
        {
            GameUIManager.instance.TriggerWinGame();
        }
    }
}
