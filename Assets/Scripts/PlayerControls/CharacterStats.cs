using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("Temel İstatistikler")]
    public int maxHealth = 100;
    public int currentHealth;
    public bool isDead; 

    public float totalPoiseDefense = 30f; // Denge (Poise) değeri
    public float currentPoiseDefense;

    public virtual void Start()
    {
        // Başlangıçta canı fulle
        currentHealth = maxHealth;
        currentPoiseDefense = totalPoiseDefense;
    }

    /// <summary>
    /// Evrensel hasar alma fonksiyonu. Tüm karakterler (Oyuncu ve Düşman) bunu kullanır.
    /// Alt sınıflarda (Override) özelleştirilebilir.
    /// </summary>
    public virtual void TakeDamage(int damage, float poiseDamage, float knockbackForce, string damageAnimation = "Damage", Transform damageSource = null, Vector3 hitPoint = default)
    {
        currentHealth = currentHealth - damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            HandleDeath();
        }
    }

    /// <summary>
    /// Karakter öldüğünde çalışacak fonksiyon. Alt sınıflarda doldurulmalı.
    /// </summary>
    public virtual void HandleDeath()
    {
        // Base logic or empty for override
    }
}
