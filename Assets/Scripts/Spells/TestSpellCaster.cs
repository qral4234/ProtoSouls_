using UnityEngine;
using UnityEngine.UI; // UI için gerekli

public class TestSpellCaster : MonoBehaviour
{
    [Header("Spell Setup")]
    public GameObject tornadoPrefab; 
    public Transform castPoint;

    [Header("Cooldown")]
    public float cooldownTime = 5f; // 5 saniye bekleme süresi
    private float currentCooldownTimer = 0;
    public Image cooldownUI; // Unity'den buraya bir Image sürükleyeceksin (Image Type: Filled)

    void Update()
    {
        HandleCooldown();

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // Cooldown dolduysa at
            if (currentCooldownTimer <= 0)
            {
                if (tornadoPrefab != null)
                {
                    CastTornado();
                    currentCooldownTimer = cooldownTime; // Sayacı başlat
                }
                else
                {
                    Debug.LogError("Tornado Prefab atanmamış!");
                }
            }
            else
            {
                Debug.Log("Büyü doluyor... Bekle!");
            }
        }
    }

    private void HandleCooldown()
    {
        if (currentCooldownTimer > 0)
        {
            currentCooldownTimer -= Time.deltaTime;
            
            // UI Güncelleme (Simit gibi dolması için)
            if (cooldownUI != null)
            {
                cooldownUI.fillAmount = currentCooldownTimer / cooldownTime;
            }
        }
        else
        {
             // Doldu, UI'ı temizle
             if (cooldownUI != null)
             {
                 cooldownUI.fillAmount = 0;
             }
        }
    }

    void CastTornado()
    {
        Vector3 spawnPos = transform.position + transform.forward * 1.5f + Vector3.up * 1.0f;
        Quaternion spawnRot = transform.rotation;

        if (castPoint != null)
        {
            spawnPos = castPoint.position;
            spawnRot = transform.rotation; 
        }

        Instantiate(tornadoPrefab, spawnPos, spawnRot);
    }
}
