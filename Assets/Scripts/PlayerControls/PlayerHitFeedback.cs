using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitFeedback : MonoBehaviour
{
    [Header("Parlam Ayarları")]
    [SerializeField] private Color flashColor = Color.red; // Yanıp sönecek renk
    [SerializeField] private float flashDuration = 0.15f;

    // Oyuncunun üzerindeki tüm parçaları (Kafa, Gövde, Zırh) tutan liste
    private List<SkinnedMeshRenderer> skinnedRenderers = new List<SkinnedMeshRenderer>();
    private List<Color> originalColors = new List<Color>();
    
    private Coroutine flashCoroutine;

    private void Awake()
    {
        // Tüm alt objelerdeki rendererları bul
        SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var rend in renderers)
        {
            skinnedRenderers.Add(rend);
        }
    }

    private void Start()
    {
        // Her parçanın kendi orijinal rengini kaydet
        foreach (var rend in skinnedRenderers)
        {
            if (rend.material.HasProperty("_Color"))
                originalColors.Add(rend.material.color);
            else if (rend.material.HasProperty("_BaseColor"))
                originalColors.Add(rend.material.GetColor("_BaseColor"));
            else
                originalColors.Add(Color.white); 
        }
    }

    public void PlayHitFeedback()
    {
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(MaterialFlash());
    }

    private IEnumerator MaterialFlash()
    {
        // Hepsini Kırmızı Yap
        foreach (var rend in skinnedRenderers)
        {
            if (rend != null)
                rend.material.color = flashColor;
        }

        // Bekle
        yield return new WaitForSeconds(flashDuration);

        // Orijinale Dön
        for (int i = 0; i < skinnedRenderers.Count; i++)
        {
            if (skinnedRenderers[i] != null && i < originalColors.Count)
            {
                skinnedRenderers[i].material.color = originalColors[i];
            }
        }

        flashCoroutine = null;
    }
}
