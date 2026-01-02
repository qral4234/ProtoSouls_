using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class TornadoVisuals : MonoBehaviour
{
    void Start()
    {
        SetupTornadoParticles();
    }

    void SetupTornadoParticles()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();

        // --- FIX 1: Hata Çözümü ---
        // Ayarları değiştirmeden önce sistemi tamamen durdur ve temizle
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystemRenderer psRenderer = GetComponent<ParticleSystemRenderer>();
        
        // --- FIX 2: Daha İyi Saydamlık (Alpha Blended) ---
        // "Mobile/Particles/Alpha Blended" shader'ı varsayılan olarak en iyi saydamlığı verir
        // ve texture olmasa bile yumuşak gözükür.
        // FIX: "Sprites/Default" buildde kesinlikle bulunur. Görünmezlik sorununu çözer.
        Material defaultMat = new Material(Shader.Find("Sprites/Default")); 
        if(defaultMat != null)
        {
            psRenderer.material = defaultMat;
        }

        ParticleSystem.MainModule main = ps.main;
        ParticleSystem.ShapeModule shape = ps.shape;
        ParticleSystem.VelocityOverLifetimeModule velocity = ps.velocityOverLifetime;
        ParticleSystem.EmissionModule emission = ps.emission;
        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = ps.colorOverLifetime;
        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = ps.sizeOverLifetime;

        // --- Main Settings ---
        main.duration = 5f;
        main.loop = true;
        main.startLifetime = 1.2f; // Biraz daha hızlı yok olsunlar, daha dinamik durur
        main.startSpeed = 0f; 
        main.startSize = new ParticleSystem.MinMaxCurve(1f, 2.5f); 
        // Alpha: 0.05 -> 0.3 (Daha görünür)
        // Renk: Hafif mavi/turkuaz (Magical Air)
        // Alpha: 0.15 çok düşüktü, 0.6 yapıyoruz ki net görülsün
        // Eğer shader opaque ise Solid renk olacak ama en azından görünecek.
        main.startColor = new Color(0.6f, 0.8f, 1.0f, 0.6f); 
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 300; 

        // --- Emission ---
        // 30 -> 60 (Daha yoğun hatlar)
        emission.rateOverTime = 60f; 

        // --- Shape ---
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 20f; // Açıyı biraz kıstım, daha sivri olsun
        shape.radius = 0.5f; 
        shape.radiusThickness = 0.1f; // Tamamen boş olmasın, azıcık kalınlık verdim
        shape.rotation = new Vector3(-90f, 0f, 0f);

        // --- Velocity & Movement ---
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        
        velocity.x = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocity.y = new ParticleSystem.MinMaxCurve(4f, 8f); // Çok daha hızlı yükselsin (Rüzgar Hissi)
        
        velocity.orbitalX = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocity.orbitalZ = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocity.orbitalY = new ParticleSystem.MinMaxCurve(10f, 20f); // Dönüş hızı arttı

        // --- Visual Fixes ---
        psRenderer.minParticleSize = 0.0f;
        psRenderer.maxParticleSize = 10f; 
        psRenderer.renderMode = ParticleSystemRenderMode.Billboard;

        // --- Color Over Lifetime ---
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(0.7f, 0.9f, 1f), 0.0f), new GradientColorKey(Color.white, 1.0f) },
            // Alpha Grafiği: Daha net görünürlük için %40 yerine %80 yapıldı
            new GradientAlphaKey[] { new GradientAlphaKey(0.2f, 0.0f), new GradientAlphaKey(0.8f, 0.5f), new GradientAlphaKey(0f, 1.0f) }
        );
        colorOverLifetime.color = gradient;
        
        // --- Size Over Lifetime ---
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0.0f, 0.5f);
        curve.AddKey(1.0f, 4.0f); // Giderek devleşsin
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, curve);

        // --- FIX : Start System ---
        // Ayarlar bitti, şimdi başlat
        ps.Play();
    }
}
