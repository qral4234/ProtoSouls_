using UnityEngine;

public class EnemyStats : CharacterStats
{
    Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        // Optionally set Enemy specific health here
    }

    public override void TakeDamage(int damage, string damageAnimation = "Damage", Transform damageSource = null)
    {
        base.TakeDamage(damage, damageAnimation, damageSource);

        if (animator != null)
        {
            animator.SetTrigger("TakeDamage");
        }
    }

    public override void HandleDeath()
    {
        currentHealth = 0;
        if (animator != null)
        {
            animator.SetBool("isDead", true);
        }

        // Fix for falling through floor: Stop physics, then disable collision
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
    }
}
