using UnityEngine;
using UnityEngine.AI;

public class EnemyManager : MonoBehaviour
{
    // --- BİLEŞEN REFERANSLARI ---
    EnemyStats enemyStats;
    NavMeshAgent navMeshAgent; // Yapay zeka hareket motoru
    Animator animator;
    
    [Header("Hedef Bilgileri")]
    public Transform currentTarget; // Kovaladığımız hedef (Oyuncu)
    public float distanceFromTarget; // Hedefe olan mesafe
    public float stoppingDistance = 1.5f; // Hedefe ne kadar yaklaşınca duralım?
    public float rotationSpeed = 15f;
    
    [Tooltip("Düşman şu an bir aksiyon (saldırı vb.) yapıyor mu?")]
    public bool isPreformingAction;

    [Header("Saldırı Ayarları")]
    public float currentRecoveryTime = 0; // Saldırı yaptıktan sonraki bekleme süresi
    public float attackRange = 1.5f;

    // Ellerindeki silah collider'ları
    public DamageCollider rightHandDamageCollider;
    public DamageCollider leftHandDamageCollider;

    [Header("Müzik ve Agresiflik")]
    public float detectionRadius = 15f; // Oyuncuyu fark etme mesafesi
    public bool isInCombatMode; // Savaş modunda mı?
    public AudioSource bossMusicSource;
    [Range(0,1)] public float musicVolume = 0.2f;

    [Header("Ses Efektleri")]
    public AudioClip punchSound;
    [Range(0,1)] public float punchVolume = 1f;

    public AudioClip roarSound;
    [Range(0,1)] public float roarVolume = 1f;

    private bool hasRoared = false; // Zafer kükremesi tek sefer çalsın diye
    AudioSource sfxAudioSource;

    public PlayerManager targetPlayerManager; // Hedefin scripti

    private void Awake()
    {
        enemyStats = GetComponent<EnemyStats>();
        navMeshAgent = GetComponentInChildren<NavMeshAgent>();
        
        // AI rotasyonunu NavMeshAgent değil, biz manuel yapacağız.
        if(navMeshAgent != null)
        {
            navMeshAgent.updateRotation = false;
        }

        animator = GetComponentInChildren<Animator>();
        
        // Oyuncuyu bul
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            currentTarget = player.transform;
            targetPlayerManager = player.GetComponent<PlayerManager>();
        }

        // Ses kaynağını oluştur
        sfxAudioSource = gameObject.AddComponent<AudioSource>();
        sfxAudioSource.spatialBlend = 1f; // 3D Ses
    }

    private void Update()
    {
        HandleRecoveryTimer();
        HandleCurrentAction();
    }

    // Bekleme süresini (Cooldown) geri sayar
    private void HandleRecoveryTimer()
    {
        if (currentRecoveryTime > 0)
        {
            currentRecoveryTime -= Time.deltaTime;
        }

        if (isPreformingAction)
        {
            // Bekleme süresi azaldıkça aksiyon bayrağını indir
            if (currentRecoveryTime < 1.0f)
            {
                isPreformingAction = false;
            }
        }
    }

    // Düşmanın ana yapay zeka mantığı
    private void HandleCurrentAction()
    {
        if (enemyStats.currentHealth <= 0) return; // Ölü ise işlem yapma
        if (currentTarget == null) return; // Hedef yoksa dur

        // Oyuncu öldüyse -> Zafer
        if (targetPlayerManager != null && targetPlayerManager.isDead)
        {
             navMeshAgent.enabled = false;
             isPreformingAction = true;
             animator.SetBool("isWinner", true); // Zafer animasyonu

             if (!hasRoared && roarSound != null)
             {
                 hasRoared = true;
                 sfxAudioSource.PlayOneShot(roarSound, roarVolume);
                 if(bossMusicSource != null) bossMusicSource.Stop();
             }
             return;
        }

        distanceFromTarget = Vector3.Distance(currentTarget.position, transform.position);

        // --- MÜZİK VE SAVAŞ MODU ---
        // Yakındaysa müziği başlat
        if (!isInCombatMode && distanceFromTarget <= detectionRadius)
        {
            isInCombatMode = true;
            if(bossMusicSource != null && !bossMusicSource.isPlaying)
            {
                 bossMusicSource.volume = musicVolume;
                 bossMusicSource.Play();
            }
        }
        // Uzaklaşırsa müziği durdur
        else if (isInCombatMode && distanceFromTarget > detectionRadius * 2f)
        {
             isInCombatMode = false;
             if(bossMusicSource != null && bossMusicSource.isPlaying)
             {
                 bossMusicSource.Stop();
             }
        }

        // --- HAREKET VE SALDIRI ---
        
        // Hedefe uzaksa -> Yürü
        if (distanceFromTarget > stoppingDistance)
        {
            if (isPreformingAction)
            {
                navMeshAgent.enabled = false; // Saldırırken yürüme
            }
            else
            {
                navMeshAgent.enabled = true;

                if (navMeshAgent.isActiveAndEnabled)
                {
                    navMeshAgent.SetDestination(currentTarget.position);
                    animator.SetFloat("Vertical", 1, 0.1f, Time.deltaTime); // Yürüme animasyonu
                    
                    RotateTowardsTarget(); // Hedefe bak
                }
            }
        }
        // Hedefe yakınsa -> Saldır
        else
        {
            navMeshAgent.enabled = false;
            animator.SetFloat("Vertical", 0, 0.1f, Time.deltaTime); // Durma animasyonu
            RotateTowardsTarget();

            if (!isPreformingAction)
            {
                AttackTarget();
            }
        }
    }

    // Rastgele saldırı seç ve uygula
    private void AttackTarget()
    {
        if (currentRecoveryTime > 0) return; // Cooldown daysak saldırma
        if (distanceFromTarget > attackRange) return; // Menzil dışındaysak saldırma

        isPreformingAction = true;
        
        currentRecoveryTime = Random.Range(1.5f, 3.0f); // Rastgele bekleme süresi ata

        // Saldırı seçimi (0 veya 1)
        int randomAttack = Random.Range(0, 2);
        animator.SetInteger("AttackIndex", randomAttack);
        animator.SetTrigger("isAttacking");
        
        // Seçilen saldırıya göre silahın "Vuruş Animasyonu" adını ayarla
        if (randomAttack == 0) 
        { 
            rightHandDamageCollider.currentHitAnimation = "GetHit_01"; 
            leftHandDamageCollider.currentHitAnimation = "GetHit_01";
            PlayPunchSound(); 
        }
        else if (randomAttack == 1) 
        { 
            rightHandDamageCollider.currentHitAnimation = "GetHit_02"; 
            leftHandDamageCollider.currentHitAnimation = "GetHit_02";
            PlayPunchSound(); 
        }
    }
    
    public void PlayPunchSound()
    {
        if(punchSound != null && sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(punchSound, punchVolume);
        }
    }

    // NavMeshAgent yerine manuel rotasyon (Daha pürüzsüz)
    private void RotateTowardsTarget()
    {
        if (isPreformingAction) return;

        Vector3 direction = currentTarget.position - transform.position;
        direction.y = 0; // Sadece Y ekseninde (sağa/sola) dönsün
        
        if (direction == Vector3.zero)
            direction = transform.forward;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    // --- ANIMASYON EVENTLERİ (Animator tarafından çağrılır) ---
    // Yumruk atarken elindeki collider'ı açar/kapatır
    #region Animation Events
    public void OpenRightDamageCollider()
    {
        rightHandDamageCollider.EnableDamageCollider();
    }

    public void CloseRightDamageCollider()
    {
        rightHandDamageCollider.DisableDamageCollider();
    }

    public void OpenLeftDamageCollider()
    {
        leftHandDamageCollider.EnableDamageCollider();
    }

    public void CloseLeftDamageCollider()
    {
        leftHandDamageCollider.DisableDamageCollider();
    }
    #endregion
}
