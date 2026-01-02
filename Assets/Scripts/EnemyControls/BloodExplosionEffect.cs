using UnityEngine;

public class BloodExplosionEffect : MonoBehaviour
{
    private ParticleSystem bloodParticles;

    public void Explode()
    {
        if (bloodParticles == null)
        {
            CreateBloodSystem();
        }

        bloodParticles.Play(); // Patlat
    }

    // Kod ile dinamik partikül sistemi oluşturur
    private void CreateBloodSystem()
    {
        GameObject go = new GameObject("BloodParticleSystem");
        go.transform.position = transform.position + Vector3.up * 1.5f; // Göğüs hizası
        go.transform.parent = null; // Düşman ölünce yok olmasın diye parent'tan çıkar

        bloodParticles = go.AddComponent<ParticleSystem>();
        ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();

        var main = bloodParticles.main;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(2f, 4f); 
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 15f);   
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.4f); 
        main.gravityModifier = 2f; 
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 200;

        main.startColor = new Color(0.85f, 0.05f, 0.05f, 1f); // Kan kırmızısı

        var emission = bloodParticles.emission;
        emission.enabled = true;
        emission.rateOverTime = 0; 
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 150) }); // Tek seferde patlama

        var shape = bloodParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f; 

        var collision = bloodParticles.collision;
        collision.enabled = true;
        collision.type = ParticleSystemCollisionType.World; // Yere çarpınca dursun
        collision.mode = ParticleSystemCollisionMode.Collision3D;
        collision.dampen = 0.7f; 
        collision.bounce = 0.1f; 
        collision.lifetimeLoss = 0.5f; 

        var trails = bloodParticles.trails;
        trails.enabled = true;
        trails.ratio = 0.8f; 
        trails.lifetime = new ParticleSystem.MinMaxCurve(0.15f); 
        trails.dieWithParticles = true;
        trails.widthOverTrail = new ParticleSystem.MinMaxCurve(1f, 0.5f); 

        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        
        renderer.material = new Material(Shader.Find("Sprites/Default")); // FIX: Build'de garanti olan Shader
        renderer.material.color = new Color(0.9f, 0.1f, 0.1f, 1f); 
        
        renderer.trailMaterial = new Material(Shader.Find("Sprites/Default")); // FIX: Trail için de aynısı
        renderer.trailMaterial.color = new Color(0.9f, 0.1f, 0.1f, 0.8f); 

        Destroy(go, 5f); // 5 saniye sonra temizle
    }
}
