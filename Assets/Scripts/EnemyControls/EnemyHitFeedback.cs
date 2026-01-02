using System.Collections;
using UnityEngine;

public class EnemyHitFeedback : MonoBehaviour
{
    [Header("Bağımlılıklar")]
    [SerializeField] private SkinnedMeshRenderer targetRenderer; // Rengi değişecek mesh
    [SerializeField] private Transform childMeshTransform; // Titreyecek obje

    [Header("Hit Stop (Vuruş Duraksaması)")]
    [SerializeField] private float hitStopScale = 0.05f; // Zaman ne kadar yavaşlasın?
    [SerializeField] private float hitStopDuration = 0.1f; // Ne kadar sürsün?

    [Header("Visual Shake (Görsel Titreme)")]
    [SerializeField] private float shakeIntensity = 0.1f; // Titreme şiddeti
    [SerializeField] private float shakeDuration = 0.15f;

    [Header("Material Flash (Beyaz Parlama)")]
    [SerializeField] private Color flashColor = Color.red; // Yanıp söneceği renk
    [SerializeField] private float flashDuration = 0.1f;

    private Color originalColor;
    private Vector3 originalLocalPos;
    
    // Coroutine referansları (Üst üste binmemesi için)
    private Coroutine hitStopCoroutine;
    private Coroutine shakeCoroutine;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        if (targetRenderer == null) targetRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        if (childMeshTransform == null && targetRenderer != null) childMeshTransform = targetRenderer.transform;
    }

    private void Start()
    {
        // Orijinal rengi ve pozisyonu kaydet
        if (targetRenderer != null)
        {
            if (targetRenderer.material.HasProperty("_Color"))
                originalColor = targetRenderer.material.color;
            else if (targetRenderer.material.HasProperty("_BaseColor"))
                originalColor = targetRenderer.material.GetColor("_BaseColor");
        }

        if (childMeshTransform != null)
        {
            originalLocalPos = childMeshTransform.localPosition;
        }
    }

    // Ana tetikleyici fonksiyon
    public void PlayHitFeedback()
    {
        // Önceki efektleri durdur
        if (hitStopCoroutine != null) StopCoroutine(hitStopCoroutine);
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);

        // Hepsini başlat
        hitStopCoroutine = StartCoroutine(HitStopWithFreeze());
        shakeCoroutine = StartCoroutine(VisualShake());
        flashCoroutine = StartCoroutine(MaterialFlash());
    }

    // Opsiyonel: Kan fışkırması
    public void PlayBloodSpray(Vector3 hitPosition, Vector3 direction)
    {
        GameObject sprayGO = new GameObject("BloodSpray");
        sprayGO.transform.position = hitPosition;
        sprayGO.transform.LookAt(hitPosition + direction); 

        ParticleSystem ps = sprayGO.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 1f;
        main.startLifetime = 0.5f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 10f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startColor = new Color(0.85f, 0.05f, 0.05f, 1f); 
        main.gravityModifier = 1f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 15) }); 

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 15f; 
        shape.radius = 0.05f;

        var renderer = sprayGO.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.material.color = new Color(0.9f, 0.1f, 0.1f, 1f);

        ps.Play();
        Destroy(sprayGO, 2f);
    }

    // Zamanı Dondur (Hit Stop)
    private IEnumerator HitStopWithFreeze()
    {
        Time.timeScale = hitStopScale;
        yield return new WaitForSecondsRealtime(hitStopDuration); // Gerçek zamanda bekle
        Time.timeScale = 1f;
        hitStopCoroutine = null;
    }

    // Mesh'i Titret
    private IEnumerator VisualShake()
    {
        if (childMeshTransform == null) yield break;

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            childMeshTransform.localPosition = originalLocalPos + Random.insideUnitSphere * shakeIntensity;
            
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        childMeshTransform.localPosition = originalLocalPos;
        shakeCoroutine = null;
    }

    // Materyal Rengini Değiştir (Flash)
    private IEnumerator MaterialFlash()
    {
        if (targetRenderer == null) yield break;

        targetRenderer.material.color = flashColor;

        yield return new WaitForSecondsRealtime(flashDuration);

        targetRenderer.material.color = originalColor;
        flashCoroutine = null;
    }
}
