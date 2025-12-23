using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [Header("UI Ayarları")]
    [Tooltip("Unity UI Slider bileşeni. Can barı görselini kontrol eder.")]
    public Slider healthSlider;
    
    private float targetHealth;
    
    [Header("Efekt Ayarları")]
    [Tooltip("Can barının hedef değere ulaşma hızı. Değer ne kadar yüksekse o kadar hızlı dolar/boşalır.")]
    [SerializeField] private float lerpSpeed = 5f;

    public void SetMaxHealth(float maxHealth)
    {
        // Slider'ın maksimum ve anlık değerini ayarla
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
        targetHealth = maxHealth;
    }

    public void SetCurrentHealth(float currentHealth)
    {
        // Hedef can değerini güncelle (Update fonksiyonunda yumuşak geçiş yapılacak)
        targetHealth = currentHealth;
    }

    private void Update()
    {
        // Slider değerini hedef can değerine doğru zamana bağlı olarak yumuşakça kaydır (Lerp)
        if (healthSlider.value != targetHealth)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, targetHealth, lerpSpeed * Time.deltaTime);
        }
    }
}
