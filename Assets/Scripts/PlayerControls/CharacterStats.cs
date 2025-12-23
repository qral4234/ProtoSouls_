using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    public float totalPoiseDefense = 30f;
    public float currentPoiseDefense;

    public virtual void Start()
    {
        currentHealth = maxHealth;
        currentPoiseDefense = totalPoiseDefense;
    }

    public virtual void TakeDamage(int damage, float poiseDamage, float knockbackForce, string damageAnimation = "Damage", Transform damageSource = null)
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
