using UnityEngine;

public class DamageCollider : MonoBehaviour
{
    Collider damageCollider;
    public int currentWeaponDamage = 25;
    public string currentHitAnimation = "Damage";

    private void Awake()
    {
        damageCollider = GetComponent<Collider>();
        damageCollider.isTrigger = true;
        damageCollider.enabled = false;
    }

    public void EnableDamageCollider()
    {
        damageCollider.enabled = true;
    }

    public void DisableDamageCollider()
    {
        damageCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.tag == "Player" || collision.tag == "Enemy")
        {
            CharacterStats characterStats = collision.GetComponent<CharacterStats>();

            if (characterStats != null)
            {
                characterStats.TakeDamage(currentWeaponDamage, currentHitAnimation);
            }
        }
    }
}
