using UnityEngine;

public class HealingVisuals : MonoBehaviour
{
    private ParticleSystem ps;

    /// <summary>
    /// İyileşme efektini belirtilen pozisyonda oynatır.
    /// </summary>
    public void PlayHealingEffect(Vector3 position)
    {
        GameObject vfxObj = new GameObject("Heal_VFX");
        vfxObj.transform.position = position;
        
        ps = vfxObj.AddComponent<ParticleSystem>();
        
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        var emission = ps.emission;
        var shape = ps.shape;
        var colorOverLifetime = ps.colorOverLifetime;
        var sizeOverLifetime = ps.sizeOverLifetime;
        var velOverLife = ps.velocityOverLifetime;
        
        // Materyal hatasını önle
        ParticleSystemRenderer psr = vfxObj.GetComponent<ParticleSystemRenderer>();
        if (psr != null)
        {
            psr.material = new Material(Shader.Find("Sprites/Default"));
        }

        // Efekt Ayarları
        main.startLifetime = 1.0f;
        main.startSpeed = 2.0f;
        main.startSize = 0.2f;
        main.maxParticles = 50;
        main.loop = false; 
        main.playOnAwake = false;
        main.duration = 1.0f;
        
        main.startColor = new Color(1f, 0.9f, 0.2f, 1f); // Sarı/Altın rengi

        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 30) });

        velOverLife.enabled = true;
        velOverLife.x = new ParticleSystem.MinMaxCurve(0.0f, 0.0f);
        velOverLife.y = new ParticleSystem.MinMaxCurve(1.0f, 3.0f); // Yukarı doğru
        velOverLife.z = new ParticleSystem.MinMaxCurve(0.0f, 0.0f);
        velOverLife.space = ParticleSystemSimulationSpace.World;

        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0.0f, 1.0f);
        sizeCurve.AddKey(1.0f, 0.0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, sizeCurve);

        ps.Play();

        Destroy(vfxObj, 2.0f);
    }
}
