using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class RedShockwaveVisuals : MonoBehaviour
{
    void Start()
    {
        SetupShockwave();
    }

    void SetupShockwave()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();
        // FIX: Çalışırken ayar değiştirmeyi engellemek için önce durduruyoruz.
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystemRenderer psRenderer = GetComponent<ParticleSystemRenderer>();
        
        // --- Material Setup ---
        // Alpha Blended shader kullanarak net ve şeffaf bir görünüm elde edelim
        // FIX: "Particles/Standard Unlit" da buildde olmayabilir. 
        // "Sprites/Default" her Unity buildinde %100 vardır.
        Material defaultMat = new Material(Shader.Find("Sprites/Default"));
        
        // Eğer materyal oluştuysa ata
        if(defaultMat != null) 
        {
            psRenderer.material = defaultMat;
        }

        var main = ps.main;
        var shape = ps.shape;
        var emission = ps.emission;
        var colLoop = ps.colorOverLifetime;
        var sizeLoop = ps.sizeOverLifetime;

        // 1. Genel Ayarlar
        main.duration = 1f;
        main.loop = false; // Tek seferlik patlama
        main.startLifetime = 0.5f; // Yarım saniyede yok olsun (Hızlı tepki)
        main.startSpeed = 15f; // Dışarı doğru çok hızlı fırlasın
        main.startSize = 0.5f;
        main.startColor = new Color(1f, 0.2f, 0.2f, 1f); // Tam Opak (Net görülsün diye)
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;

        // 2. Şekil (Halka)
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle; // Halka şeklinde
        shape.radius = 0.1f; // Merkezden başlasın
        shape.radiusThickness = 0f; // Sadece kenardan fırlat
        shape.rotation = new Vector3(-90f, 0f, 0f); // Yere paralel olsun

        // 3. Emission (Burst)
        emission.rateOverTime = 0; // Sürekli çıkmasın
        // Anlık 100 tane partikül patlasın
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 100) });

        // 4. Renk (Kırmızıdan Siyaha/Şeffafa)
        colLoop.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.red, 0f), new GradientColorKey(new Color(0.5f, 0f, 0f), 1f) }, // Kırmızı -> Koyu Kırmızı
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) } // Görünür -> Yok
        );
        colLoop.color = grad;

        // 5. Render Modu
        psRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        psRenderer.minParticleSize = 0.1f;
        psRenderer.maxParticleSize = 2f;

        // Temizlik: Patlama bitince objeyi yok et
        ps.Play(); // FIX: Ayarlar bitti, şimdi PATLAT!
        Destroy(gameObject, 1.0f);
    }
}
