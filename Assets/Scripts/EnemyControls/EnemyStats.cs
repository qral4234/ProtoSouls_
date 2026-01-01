using UnityEngine;

public class EnemyStats : CharacterStats
{
    Animator animator;
    EnemyLocomotionManager enemyLocomotionManager;
    EnemyManager enemyManager; // Referans eklendi
    EnemyHitFeedback hitFeedback;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        enemyLocomotionManager = GetComponent<EnemyLocomotionManager>();
        enemyManager = GetComponent<EnemyManager>(); // Referansı bul
        hitFeedback = GetComponent<EnemyHitFeedback>();
    }

    public BossHealthBar bossHealthBar;

    public override void Start()
    {
        base.Start();
        if (bossHealthBar != null)
        {
            bossHealthBar.SetMaxHealth(maxHealth);
        }
    }

    [Header("Combat Reaction")]
    public int hitCount = 0; // Vuruş sayacı
    public GameObject shockwavePrefab; // Kırmızı Halka Efekti

    public override void TakeDamage(int damage, float poiseDamage, float knockbackForce, string damageAnimation = "Damage", Transform damageSource = null)
    {
        // 1. Can Azaltma (Animasyonsuz)
        currentHealth -= damage;

        if (bossHealthBar != null)
        {
            bossHealthBar.SetCurrentHealth(currentHealth);
        }

        // FIX: Biz vururken o da anında vuruşa (saldırı animasyonuna) girmesin.
        // Her hasar aldığında saldırısını biraz erteleyelim (Baskı kurma mekaniği)
        if (enemyManager != null)
        {
            enemyManager.currentRecoveryTime += 0f; // Artık hasar alınca duraksamıyor
        }

        // Ölüm Kontrolü
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            HandleDeath(); // base.HandleDeath yerine direkt override edileni çağır
            return;
        }

        // 2. Vuruş Sayacı Mantığı
        hitCount++;

        if (hitCount >= 2)
        {
            // --- MİSİLLEME ZAMANI (RETALIATION) ---
            hitCount = 0; // Sayacı sıfırla

            // A. Şok Dalgası Efektini Yarat
            CreateShockwaveEffect();

            // B. Player'ı İttir (Alana Hasar/Kuvvet Uygula)
            PushPlayerBack();
        }
        else
        {
            // --- NORMAL VURUŞ (SADECE SES/FLASH) ---
            // Animasyon oynatma! (User isteği: "vuruş animasyonuna girmesin")
            if (hitFeedback != null)
            {
                hitFeedback.PlayHitFeedback(); // Sadece materyal parlasın
            }
        }
    }

    private void CreateShockwaveEffect()
    {
        // Eğer prefab atanmadıysa çalışma anında oluştur (Senin için kolaylık olsun diye)
        if (shockwavePrefab == null)
        {
            GameObject go = new GameObject("RedShockwave");
            go.transform.position = transform.position + Vector3.up * 0.5f; // Yerden biraz yukarıda
            go.AddComponent<RedShockwaveVisuals>(); // Az önce yazdığımız script
        }
        else
        {
            Instantiate(shockwavePrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }
    }

    private void PushPlayerBack()
    {
        // Etrafındaki 5 metredeki her şeyi al
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f);

        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Player"))
            {
                // Player'ı bulduk, itelim
                PlayerLocomotion playerLocomotion = col.GetComponent<PlayerLocomotion>();
                if (playerLocomotion != null)
                {
                    // Düşmandan Player'a doğru olan yönü bul
                    Vector3 pushDir = (col.transform.position - transform.position).normalized;
                    pushDir.y = 0.2f; // Hafif yukarı kaldırsın

                    // Kuvvetli bir itiş (Güç arttırıldı: 100 -> 200)
                    playerLocomotion.ApplyKnockback(pushDir, 200f);
                }
            }
        }
    }

    public override void HandleDeath()
    {
        currentHealth = 0;
        if (animator != null)
        {
            isDead = true; // Base class değişkenini güncelle
            animator.SetBool("isDead", true);
            animator.Play("Death_01"); // Varsayılan ölüm animasyonu
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

        // FIX: Ölünce AI'yı tamamen sustur
        if(enemyLocomotionManager != null) enemyLocomotionManager.enabled = false;
        if(enemyManager != null) enemyManager.enabled = false;
    }
}
