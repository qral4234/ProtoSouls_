using UnityEngine;
using UnityEngine.UI;

public class LockOnIndicatorUI : MonoBehaviour
{
    public Image indicatorImage; // Kilit görseli (Target ikonu)

    [Header("Renk Ayarları")]
    public Color lockedColor = Color.green; // Kilitliyken (Yeşil)
    public Color unlockedColor = Color.red; // Kilitsizken (Kırmızı)

    private void Start()
    {
        if(indicatorImage == null)
            indicatorImage = GetComponent<Image>();
    }

    private void Update()
    {
        if (CameraHandler.singleton == null) return;

        // Kameranın bir hedefi olup olmadığına bak
        if (CameraHandler.singleton.currentLockOnTarget != null)
        {
            indicatorImage.color = lockedColor;
        }
        else
        {
            indicatorImage.color = unlockedColor;
        }
    }
}
