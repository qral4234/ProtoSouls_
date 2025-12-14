using UnityEngine;

public class EnemyLocomotionManager : MonoBehaviour
{
    Rigidbody enemyRigidbody;
    
    // Dependencies
    EnemyManager enemyManager;

    private void Awake()
    {
        enemyManager = GetComponent<EnemyManager>();
        enemyRigidbody = GetComponent<Rigidbody>();
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        if (enemyRigidbody == null) return;

        direction.Normalize();
        direction.y = 0; // Keep horizontal

        // Ensure physics can move the enemy
        enemyRigidbody.isKinematic = false; 

        // Apply impulse
        enemyRigidbody.AddForce(direction * force, ForceMode.Impulse);
    }
}
