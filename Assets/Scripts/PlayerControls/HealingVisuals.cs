using UnityEngine;

public class HealingVisuals : MonoBehaviour
{
    private ParticleSystem ps;

    public void PlayHealingEffect(Vector3 position)
    {
        // Geçici bir efekt objesi yarat
        GameObject vfxObj = new GameObject("Heal_VFX");
        vfxObj.transform.position = position;
        
        // Particle System ekle
        ps = vfxObj.AddComponent<ParticleSystem>();
        
        // FIX: Ayarları yapmadan önce sistemi durdur (Active bir particle üzerinde duration değiştirmek hata verir)
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        var emission = ps.emission;
        var shape = ps.shape;
        var colorOverLifetime = ps.colorOverLifetime;
        var sizeOverLifetime = ps.sizeOverLifetime;
        var velOverLife = ps.velocityOverLifetime;
        
        // FIX: Mor kare sorununu çözmek için Materyal Ata
        ParticleSystemRenderer psr = vfxObj.GetComponent<ParticleSystemRenderer>();
        if (psr != null)
        {
            // Unity'nin varsayılan particle materyali (Genellikle her projede vardır)
            psr.material = new Material(Shader.Find("Sprites/Default"));
            // Veya daha şık "Legacy Shaders/Particles/Alpha Blended" de olabilir ama Sprites/Default garantidir.
        }

        // --- AYARLAR ---
        
        // 1. Genel
        main.startLifetime = 1.0f;
        main.startSpeed = 2.0f;
        main.startSize = 0.2f;
        main.maxParticles = 50;
        main.loop = false; // Tek seferlik patlama
        main.playOnAwake = false;
        main.duration = 1.0f;
        
        // Renk (Parlak Sarı)
        main.startColor = new Color(1f, 0.9f, 0.2f, 1f); 

        // 2. Şekil (Karakterin etrafında)
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        // 3. Emisyon (Burst)
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 30) });

        // 4. Hareket (Yukarı doğru süzülme - Ruhani his)
        velOverLife.enabled = true;
        // FIX: Hata almamak için tüm eksenlerin modu aynı olmalı (Hepsi MinMaxCurve)
        velOverLife.x = new ParticleSystem.MinMaxCurve(0.0f, 0.0f);
        velOverLife.y = new ParticleSystem.MinMaxCurve(1.0f, 3.0f); // Yukarı doğru hız
        velOverLife.z = new ParticleSystem.MinMaxCurve(0.0f, 0.0f);
        velOverLife.space = ParticleSystemSimulationSpace.World;

        // 5. Boyut (Küçülerek yok ol)
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0.0f, 1.0f);
        sizeCurve.AddKey(1.0f, 0.0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, sizeCurve);

        // Oynat
        ps.Play();

        // 2 saniye sonra yok et (Efekt bitince çöp olmasın)
        Destroy(vfxObj, 2.0f);
    }
}
