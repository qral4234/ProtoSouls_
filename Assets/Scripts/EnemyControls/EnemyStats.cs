using UnityEngine;

public class EnemyStats : CharacterStats
{
    Animator animator;
    EnemyLocomotionManager enemyLocomotionManager;
    EnemyHitFeedback hitFeedback;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        enemyLocomotionManager = GetComponent<EnemyLocomotionManager>();
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

    public override void TakeDamage(int damage, float poiseDamage, float knockbackForce, string damageAnimation = "Damage", Transform damageSource = null)
    {
        if (hitFeedback != null)
        {
            hitFeedback.PlayHitFeedback();
        }

        // 1. Take Health Damage (Always) - CharacterStats handles death if health <= 0
        base.TakeDamage(damage, poiseDamage, knockbackForce, damageAnimation, damageSource);

        if (bossHealthBar != null)
        {
            bossHealthBar.SetCurrentHealth(currentHealth);
        }

        // 2. Subtract Poise
        currentPoiseDefense = currentPoiseDefense - poiseDamage;
        
        // Debugging as requested
        Debug.Log("Current Poise: " + currentPoiseDefense);

        // 3. Logic Check
        if (currentPoiseDefense > 0)
        {
            // Optional: Play Blood SFX here if you have a system for it
            // DO NOT play animation
            // DO NOT apply knockback
        }
        else // currentPoiseDefense <= 0
        {
            currentPoiseDefense = totalPoiseDefense; // Reset Poise

            // Apply Full Knockback
            if (enemyLocomotionManager != null && damageSource != null)
            {
                Vector3 knockbackDir = transform.position - damageSource.position;
                enemyLocomotionManager.ApplyKnockback(knockbackDir, knockbackForce);
            }
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
