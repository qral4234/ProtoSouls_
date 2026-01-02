using UnityEngine;

public class PlayerRageManager : MonoBehaviour
{
    InputHandler inputHandler;
    AnimatorHandler animatorHandler;

    [Header("UI")]
    public RageBar rageBar; // Öfke barı görseli

    [Header("Öfke İstatistikleri")]
    public float maxRage = 100;
    public float currentRage = 0;
    public bool isRageActive = false; // Öfke modu açık mı?
    public float damageMultiplier = 1.5f; // Öfkeliyken hasar kaç katına çıksın?

    [Header("Ceza Sistemi (Penalty Stack)")]
    public int usageStack = 0; // Kaç kere RAGE açtık? Her açışta dolum hızı düşer.
    
    [Header("Bekleme Süresi (Cooldown)")]
    public float cooldownTimer = 0;
    public float cooldownDuration = 15f;

    [Header("Harcama Ayarları")]
    public float sprintDrainAmount = 5f; // Koşarken harcanan miktar
    public float attackDrainAmount = 10f; // Saldırırken harcanan miktar

    private void Awake()
    {
        inputHandler = GetComponent<InputHandler>();
        animatorHandler = GetComponentInChildren<AnimatorHandler>();
    }

    private void Start()
    {
        // Barı sıfırla
        if (rageBar != null)
        {
            rageBar.SetMaxRage(maxRage);
            rageBar.SetCurrentRage(currentRage);
        }
    }

    private void Update()
    {
        float delta = Time.deltaTime;

        HandleCooldown(delta);
        HandleRageActivation(); // R tuşunu dinle
        HandleSprintDrain(delta); // Koşuyorsa harca
    }

    // Cooldown süresini geri sayar
    private void HandleCooldown(float delta)
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= delta;
        }
    }

    // R tuşuna basınca RAGE'i aktif eder
    private void HandleRageActivation()
    {
        // Sadece hazırsa ve doluysa
        if (!isRageActive && cooldownTimer <= 0 && currentRage >= maxRage)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                ActivateRage();
            }
        }
    }

    private ParticleSystem rageParticles; // Kırmızı aura efekti

    // Modu aç
    public void ActivateRage()
    {
        isRageActive = true;
        animatorHandler.PlayTargetAnimation("Rage_Activate", true); // Kükreme animasyonu

        if (rageParticles == null)
        {
            SetupRageParticles(); // Eşekt yoksa oluştur
        }
        rageParticles.Play();
    }

    // Modu kapat (Bittiğinde)
    public void DeactivateRage()
    {
        isRageActive = false;
        currentRage = 0;
        
        if (rageBar != null)
            rageBar.SetCurrentRage(0);

        usageStack++; // Cezayı artır
        cooldownTimer = cooldownDuration; // Cooldown başlat

        if (rageParticles != null)
        {
            rageParticles.Stop();
        }
    }

    // Partikül efektini kod ile oluştur (Prefab gerektirmez)
    private void SetupRageParticles()
    {
        GameObject go = new GameObject("Rage_VFX");
        go.transform.parent = transform; 
        go.transform.localPosition = Vector3.up * 1.0f; 

        rageParticles = go.AddComponent<ParticleSystem>();
        
        rageParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = rageParticles.main;
        main.loop = true; 
        main.startLifetime = 1.0f;
        main.startSpeed = 0.5f;
        main.startSize = 0.1f;
        main.startColor = Color.red; 
        main.simulationSpace = ParticleSystemSimulationSpace.Local; 

        var emission = rageParticles.emission;
        emission.rateOverTime = 20; 

        var shape = rageParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.8f; 

        var vel = rageParticles.velocityOverLifetime;
        vel.enabled = true;
        vel.y = new ParticleSystem.MinMaxCurve(0.2f, 1.0f); 
        vel.x = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);
        vel.z = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);

        ParticleSystemRenderer psr = go.GetComponent<ParticleSystemRenderer>();
        if (psr != null)
        {
            psr.material = new Material(Shader.Find("Sprites/Default"));
        }
    }

    // Öfke Kazandır
    public void AddRage(float amount)
    {
        if (isRageActive) return; // Zaten aktifse doldurma
        if (cooldownTimer > 0) return; // Cooldown daysa doldurma

        // Ceza sistemine göre dolum hızını azalt
        float fillMultiplier = 1.0f;
        if (usageStack == 1) fillMultiplier = 0.85f;
        else if (usageStack == 2) fillMultiplier = 0.70f;
        else if (usageStack >= 3) fillMultiplier = 0.55f;

        currentRage += amount * fillMultiplier;

        if (currentRage >= maxRage)
            currentRage = maxRage;

        if (rageBar != null)
            rageBar.SetCurrentRage(currentRage);
    }

    // Öfke Harca
    public void DrainRage(float amount)
    {
        if (!isRageActive) return;

        currentRage -= amount;

        if (currentRage <= 0)
        {
            currentRage = 0;
            DeactivateRage();
        }

        if (rageBar != null)
            rageBar.SetCurrentRage(currentRage);
    }

    // Koşarken öfke harca
    private void HandleSprintDrain(float delta)
    {
        if (isRageActive && inputHandler.sprintFlag && inputHandler.moveAmount > 0.5f)
        {
            DrainRage(sprintDrainAmount * delta);
        }
    }
}
