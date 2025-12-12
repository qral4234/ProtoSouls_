using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    public virtual void Start()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(int damage, string damageAnimation = "Damage")
    {
        currentHealth = currentHealth - damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            HandleDeath();
        }
    }

    public virtual void HandleDeath()
    {
        // Base logic or empty for override
    }
}
