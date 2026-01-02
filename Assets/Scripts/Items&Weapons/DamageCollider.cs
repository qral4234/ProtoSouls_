using UnityEngine;
using System.Collections.Generic;

public class DamageCollider : MonoBehaviour
{
    Collider damageCollider;
    
    [Header("Hasar Ayarları")]
    public int currentWeaponDamage = 25; // Silahın taban hasarı
    public string currentHitAnimation = "Damage"; // Karşı tarafta oynatılacak hasar animasyonu
    public float poiseBreakPower = 10f; // Denge bozma gücü
    public float knockbackForce = 20f; // İtme gücü

    // Aynı vuruşta aynı kişiye iki kere hasar vermemek için liste
    List<CharacterStats> charactersDamagedDuringThisSwing = new List<CharacterStats>();

    AudioSource audioSource;
    public AudioClip currentHitSound;

    private void Awake()
    {
        damageCollider = GetComponent<Collider>();
        damageCollider.isTrigger = true; // İçinden geçilebilir olmalı (Trigger)
        damageCollider.enabled = false; // Başlangıçta hasar kapalı
        
        audioSource = GetComponent<AudioSource>();
        if(audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // Vuruş başladığında çağrılır
    public void EnableDamageCollider()
    {
        damageCollider.enabled = true;
        charactersDamagedDuringThisSwing.Clear(); // Listeyi temizle
    }

    // Vuruş bittiğinde çağrılır
    public void DisableDamageCollider()
    {
        damageCollider.enabled = false;
        charactersDamagedDuringThisSwing.Clear();
    }

    // Bir şeye dokunduğunda (Trigger) çalışır
    private void OnTriggerEnter(Collider collision)
    {
        // Sadece "Player" veya "Enemy" etiketli objelere hasar ver
        if (collision.tag == "Player" || collision.tag == "Enemy")
        {
            CharacterStats characterStats = collision.GetComponent<CharacterStats>();

            if (characterStats != null)
            {
                // Eğer bu listede varsa (zaten vurduk), tekrar vurma
                if (charactersDamagedDuringThisSwing.Contains(characterStats))
                    return;

                charactersDamagedDuringThisSwing.Add(characterStats);

                // Hasar hesaplaması
                int finalDamage = currentWeaponDamage;
                
                // RAGE MODE: Öfke aktifse hasarı artır
                PlayerRageManager rageManager = GetComponentInParent<PlayerRageManager>();
                if (rageManager != null)
                {
                    rageManager.AddRage(currentWeaponDamage); // Vurdukça öfke doldur

                    if (rageManager.isRageActive)
                    {
                        finalDamage = Mathf.RoundToInt(currentWeaponDamage * rageManager.damageMultiplier);
                    }
                }

                // Saldıran kişinin kim olduğunu bul (İtme kuvveti için)
                CharacterStats attackerStats = GetComponentInParent<CharacterStats>();
                Transform attackerTransform = (attackerStats != null) ? attackerStats.transform : transform;

                // Hedefe hasarı uygula
                characterStats.TakeDamage(finalDamage, poiseBreakPower, knockbackForce, currentHitAnimation, attackerTransform);

                // Vuruş sesi çal
                if (currentHitSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(currentHitSound);
                }
            }
        }
    }
}
