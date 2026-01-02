using UnityEngine;

public class PlayerAttacker : MonoBehaviour
{
    // --- BİLEŞEN REFERANSLARI ---
    AnimatorHandler animatorHandler;
    DamageCollider damageCollider; // Silahın üzerindeki hasar verici collider
    public string lastAttack; // En son yapılan saldırının adı (Kombo için)

    public void Awake()
    {
        animatorHandler = GetComponentInChildren<AnimatorHandler>();
        damageCollider = GetComponentInChildren<DamageCollider>();
    }

    // --- SİLAH COLLIDER KONTROLÜ (Animasyon Eventleri ile çağrılır) ---
    public void OpenDamageCollider()
    {
        if (damageCollider != null)
        {
            damageCollider.EnableDamageCollider(); // Vuruş başladı, hasarı aç
        }
    }

    public void CloseDamageCollider()
    {
        if (damageCollider != null)
        {
            damageCollider.DisableDamageCollider(); // Vuruş bitti, hasarı kapat
        }
    }

    // Hafif (Light) Saldırıyı başlatır
    public void HandleLightAttack(WeaponItem weapon)
    {
        // RAGE MODE: Öfkeyi harca (Her saldırıda azalır)
        PlayerRageManager rageManager = GetComponent<PlayerRageManager>();
        if (rageManager != null) rageManager.DrainRage(rageManager.attackDrainAmount);

        // İlk saldırı animasyonunu oynat
        animatorHandler.PlayTargetAnimation(weapon.OH_Light_Attack_1, true);
        lastAttack = weapon.OH_Light_Attack_1;

        // Vuruş sesini ayarla
        if (damageCollider != null)
        {
            damageCollider.currentHitSound = weapon.hitSound1;
        }
    }

    // Kombo Mantığı (Arka arkaya saldırı)
    public void HandleCombo(WeaponItem weapon)
    {
        // Eğer kombo penceresi açıksa (önceki animasyon izin veriyorsa)
        if (animatorHandler.canDoCombo)
        {
            animatorHandler.canDoCombo = false;
            
            PlayerRageManager rageManager = GetComponent<PlayerRageManager>();
            if (rageManager != null) rageManager.DrainRage(rageManager.attackDrainAmount);
            
            // Hangi saldırıdan sonra hangisi gelecek?
            if (lastAttack == weapon.OH_Light_Attack_1)
            {
                // 1 -> 2
                animatorHandler.PlayTargetAnimation(weapon.OH_Light_Attack_2, true);
                lastAttack = weapon.OH_Light_Attack_2;
                if (damageCollider != null) damageCollider.currentHitSound = weapon.hitSound2;
            }
            else if (lastAttack == weapon.OH_Light_Attack_2)
            {
                // 2 -> 3
                animatorHandler.PlayTargetAnimation(weapon.OH_Light_Attack_3, true);
                lastAttack = weapon.OH_Light_Attack_3;
                if (damageCollider != null) damageCollider.currentHitSound = weapon.hitSound3;
            }
        }
    }
}
