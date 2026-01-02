using UnityEngine;

public class EnemyLocomotionManager : MonoBehaviour
{
    Rigidbody enemyRigidbody;
    EnemyManager enemyManager;

    private void Awake()
    {
        enemyManager = GetComponent<EnemyManager>();
        enemyRigidbody = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Düşmana fiziksel geri tepme (Knockback) uygular.
    /// </summary>
    public void ApplyKnockback(Vector3 direction, float force)
    {
        if (enemyRigidbody == null) return;

        direction.Normalize();
        direction.y = 0; // Havaya uçmasın, sadece geriye gitsin

        enemyRigidbody.isKinematic = false; // Fiziği aç

        enemyRigidbody.AddForce(direction * force, ForceMode.Impulse);
    }
}
