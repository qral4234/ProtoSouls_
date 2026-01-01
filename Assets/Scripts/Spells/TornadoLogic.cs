using UnityEngine;
using System.Collections.Generic;

public class TornadoLogic : MonoBehaviour
{
    [Header("Tornado Settings")]
    [Tooltip("Kasırganın ileri gitme hızı")]
    public float forwardSpeed = 5f;

    [Tooltip("Düşmanları itme gücü (Yavaşça sürüklemek için düşük tutabilirsin)")]
    public float pushForce = 20f;

    [Tooltip("Kasırganın etki yarıçapı")]
    public float effectRadius = 3f;

    [Tooltip("Kasırganın kaç saniye sonra yok olacağı")]
    public float lifeTime = 5f;

    [Header("Layer Config")]
    [Tooltip("Hangi layerlar itilecek? (Genelde 'Enemy' veya 'Default')")]
    public LayerMask pushLayers;

    private void Start()
    {
        // Belirlenen süre sonra yok et (Memory yönetimi)
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // Kasırgayı sürekli ileri taşı
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        // Etki alanındaki çarpışanları bul
        Collider[] colliders = Physics.OverlapSphere(transform.position, effectRadius, pushLayers);

        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Player")) continue;

            // 1. Durum: EnemyLocomotionManager var mı? (En sağlıklı yöntem)
            EnemyLocomotionManager enemyLocomotion = col.GetComponent<EnemyLocomotionManager>();
            if (enemyLocomotion != null)
            {
                Vector3 pushDirection = (col.transform.position - transform.position).normalized;
                pushDirection.y = 0.2f; // Hafif yukarı kaldır
                
                // Senin locomotion scriptindeki ApplyKnockback fonksiyonunu kullanıyoruz
                // Not: Time.deltaTime ile çarpma, AddForce gibi düşün
                enemyLocomotion.ApplyKnockback(pushDirection, pushForce); 
            }
            // 2. Durum: Sadece Rigidbody var
            else
            {
                Rigidbody rb = col.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Kinematik ise gücü kabul etmez, geçici olarak kapat
                    if (rb.isKinematic) rb.isKinematic = false;

                    Vector3 pushDirection = (col.transform.position - transform.position).normalized;
                    pushDirection += Vector3.up * 0.5f;

                    rb.AddForce(pushDirection * pushForce, ForceMode.Impulse); // Force yerine Impulse daha ani tepki verir
                }
            }
        }
    }

    // Editörde etki alanını görmek için gizmo çizgisi
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, effectRadius);
    }
}
