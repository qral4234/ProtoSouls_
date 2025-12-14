using UnityEngine;
using System.Collections.Generic;

public class DamageCollider : MonoBehaviour
{
    Collider damageCollider;
    public int currentWeaponDamage = 25;
    public string currentHitAnimation = "Damage";
    public float knockbackForce = 15f;
    List<CharacterStats> charactersDamagedDuringThisSwing = new List<CharacterStats>();

    private void Awake()
    {
        damageCollider = GetComponent<Collider>();
        damageCollider.isTrigger = true;
        damageCollider.enabled = false;
    }

    public void EnableDamageCollider()
    {
        damageCollider.enabled = true;
        charactersDamagedDuringThisSwing.Clear();
    }

    public void DisableDamageCollider()
    {
        damageCollider.enabled = false;
        charactersDamagedDuringThisSwing.Clear();
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.tag == "Player" || collision.tag == "Enemy")
        {
            CharacterStats characterStats = collision.GetComponent<CharacterStats>();

            if (characterStats != null)
            {
                if (charactersDamagedDuringThisSwing.Contains(characterStats))
                    return;

                charactersDamagedDuringThisSwing.Add(characterStats);

                // Define Variables
                int finalDamage = currentWeaponDamage;
                PlayerRageManager rageManager = GetComponentInParent<PlayerRageManager>();

                // RAGE LOGIC
                if (rageManager != null)
                {
                    rageManager.AddRage(currentWeaponDamage);

                    if (rageManager.isRageActive)
                    {
                        finalDamage = Mathf.RoundToInt(currentWeaponDamage * rageManager.damageMultiplier);
                    }
                }

                // Pass the attacker's transform
                CharacterStats attackerStats = GetComponentInParent<CharacterStats>();
                Transform attackerTransform = (attackerStats != null) ? attackerStats.transform : transform;

                characterStats.TakeDamage(finalDamage, currentHitAnimation, attackerTransform);

                // KNOCKBACK LOGIC
                EnemyLocomotionManager enemyLocomotion = collision.GetComponent<EnemyLocomotionManager>();
                if (enemyLocomotion != null)
                {
                    Vector3 knockbackDir = collision.transform.position - attackerTransform.position;
                    enemyLocomotion.ApplyKnockback(knockbackDir, knockbackForce);
                }
            }
        }
    }
}
