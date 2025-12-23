using System.Collections;
using UnityEngine;

public class EnemyHitFeedback : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private SkinnedMeshRenderer targetRenderer;
    [SerializeField] private Transform childMeshTransform;

    [Header("Hit Stop Settings")]
    [SerializeField] private float hitStopScale = 0.05f;
    [SerializeField] private float hitStopDuration = 0.1f;

    [Header("Visual Shake Settings")]
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private float shakeDuration = 0.15f;

    [Header("Material Flash Settings")]
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;

    private Color originalColor;
    private Vector3 originalLocalPos;
    
    private Coroutine hitStopCoroutine;
    private Coroutine shakeCoroutine;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        // Cache references
        if (targetRenderer == null) targetRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        if (childMeshTransform == null && targetRenderer != null) childMeshTransform = targetRenderer.transform;
    }

    private void Start()
    {
        // Store original material color
        if (targetRenderer != null)
        {
            if (targetRenderer.material.HasProperty("_Color"))
                originalColor = targetRenderer.material.color;
            else if (targetRenderer.material.HasProperty("_BaseColor"))
                originalColor = targetRenderer.material.GetColor("_BaseColor");
        }

        // Store original local position
        if (childMeshTransform != null)
        {
            originalLocalPos = childMeshTransform.localPosition;
        }
    }

    public void PlayHitFeedback()
    {
        // Reset/Stop previous effects if a new hit occurs
        if (hitStopCoroutine != null) StopCoroutine(hitStopCoroutine);
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);

        hitStopCoroutine = StartCoroutine(HitStopWithFreeze());
        shakeCoroutine = StartCoroutine(VisualShake());
        flashCoroutine = StartCoroutine(MaterialFlash());
    }

    private IEnumerator HitStopWithFreeze()
    {
        Time.timeScale = hitStopScale;
        yield return new WaitForSecondsRealtime(hitStopDuration);
        Time.timeScale = 1f;
        hitStopCoroutine = null;
    }

    private IEnumerator VisualShake()
    {
        if (childMeshTransform == null) yield break;

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            // Shake logic: vibrate randomly within a sphere
            childMeshTransform.localPosition = originalLocalPos + Random.insideUnitSphere * shakeIntensity;
            
            // Use unscaled delta time so shake animates smoothly even if time is slowed
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Reset position
        childMeshTransform.localPosition = originalLocalPos;
        shakeCoroutine = null;
    }

    private IEnumerator MaterialFlash()
    {
        if (targetRenderer == null) yield break;

        // Apply flash color
        targetRenderer.material.color = flashColor;

        // Wait for flash duration (unscaled to remain consistent regardless of hit stop)
        yield return new WaitForSecondsRealtime(flashDuration);

        // Revert color
        targetRenderer.material.color = originalColor;
        flashCoroutine = null;
    }
}
