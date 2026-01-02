using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    // Singleton: Sahnenin her yerinden CameraHandler.singleton diyerek ulaşmak için
    public static CameraHandler singleton;

    [Header("Hedef Takibi")]
    [Tooltip("Kameranın takip edeceği hedef (Genellikle Oyuncu).")]
    public Transform targetTransform;
    [Tooltip("Kamera objesinin transformu (Asıl kamera).")]
    public Transform cameraTransform;
    [Tooltip("Kamera pivot noktası (Yükseklik ayarı ve dikey dönüş için).")]
    public Transform cameraPivotTransform;
    
    // Özel değişkenler
    private Transform myTransform;
    private Vector3 cameraTransformPosition;
    private LayerMask ignoreLayers; // Kameranın içinden geçip görmezden geleceği layerlar
    private Vector3 cameraFollowVelocity = Vector3.zero;

    [Header("Kamera Hareket Ayarları")]
    [Tooltip("Kameranın yatay dönüş hızı (Mouse X).")]
    public float lookSpeed = 0.1f;
    [Tooltip("Kameranın hedefini takip etme hızı (Yumuşatma).")]
    public float followSpeed = 0.1f;
    [Tooltip("Kameranın dikey dönüş hızı (Mouse Y).")]
    public float pivotSpeed = 0.03f;

    private float targetPosition;
    private float defaultPosition;
    private float lookAngle; // Yatay açı
    private float pivotAngle; // Dikey açı
    
    [Header("Kamera Sınırları")]
    [Tooltip("Kameranın aşağı bakabileceği maksimum açı (Negatif).")]
    public float minPivot = -35;
    [Tooltip("Kameranın yukarı bakabileceği maksimum açı (Pozitif).")]
    public float maxPivot = 35;

    [Header("Kamera Çarpışma (Collision) Ayarları")]
    [Tooltip("Kameranın duvarlara çarpmaması için kullanılan küre yarıçapı.")]
    public float cameraCollisionRadius = 0.2f;
    [Tooltip("Duvara çarpınca ne kadar öne çekileceği (Offset).")]
    public float cameraCollisionOffset = 0.2f;
    [Tooltip("Minimum yaklaşma mesafesi.")]
    public float minimumCollisionOffset = 0.2f;
    [Tooltip("Hangi layerların kamerayı engelleyeceğini belirler (Duvar, Zemin).")]
    public LayerMask collisionLayers;

    [Header("Kilitlenme (Lock-On) Sistemi")]
    [Tooltip("Şu anda kilitlenilen düşman hedefi.")]
    public Transform currentLockOnTarget;

    // OPTIMIZATION: InputHandler'ı her karede aramamak için cache'liyoruz
    InputHandler inputHandler; 

    private void Awake()
    {
        singleton = this;
        myTransform = transform;
        defaultPosition = cameraTransform.localPosition.z; // Varsayılan uzaklığı kaydet
        // Layer Mask ayarı: Player(8), PlayerLocal(9), Enemy(10) kamerayı engellemesin (~ tersini alır)
        ignoreLayers = ~(1 << 8 | 1 << 9 | 1 << 10);
        collisionLayers = ~(1 << 8 | 1 << 9 | 1 << 10); 
    }

    private void Start()
    {
        // Oyunu başlatınca InputHandler'ı bul ve sakla
        inputHandler = FindFirstObjectByType<InputHandler>();
    }

    // Kameranın hedefi (oyuncuyu) takip etmesi
    public void FollowTarget(float delta)
    {
        // Yumuşak geçiş (Lerp) ile pozisyonu güncelle
        Vector3 targetPosition = Vector3.Lerp(myTransform.position, targetTransform.position, delta / followSpeed);
        myTransform.position = targetPosition;
    }

    // Kilitlenme tuşuna basıldığında çalışır
    public void HandleLockOn()
    {
        if(inputHandler == null) return;

        if (inputHandler.lockOn_Input)
        {
            // Eğer zaten bir hedef varsa -> Kilidi Kapat
            if (currentLockOnTarget != null)
            {
                currentLockOnTarget = null;
            }
            // Hedef yoksa -> En yakın düşmanı bul ve Kilitle
            else
            {
                currentLockOnTarget = GetNearestTarget();
            }
        }
    }

    // Etraftaki düşmanları tarar ve en uygununu döndürür
    private Transform GetNearestTarget()
    {
        // 15 birim yarıçapındaki tüm colliderları al
        Collider[] colliders = Physics.OverlapSphere(targetTransform.position, 15);
        float shortestDistance = Mathf.Infinity;
        Transform nearestTarget = null;

        foreach (var hit in colliders)
        {
            if (hit.CompareTag("Enemy"))
            {
                float distance = Vector3.Distance(targetTransform.position, hit.transform.position);
                
                // Düşman önümüzde mi? (Görüş açısı kontrolü)
                Vector3 direction = hit.transform.position - targetTransform.position;
                float angle = Vector3.Angle(cameraTransform.forward, direction);
                
                // Eğer görüş açısındaysa ve en yakınsa onu seç
                if (angle < 50 && distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestTarget = hit.transform;
                }
            }
        }
        return nearestTarget;
    }

    // Kameranın dönüş (Rotation) işlemleri
    public void HandleCameraRotation(float delta, float mouseXInput, float mouseYInput)
    {
        if (delta <= 0) return; // Zaman durduysa işlem yapma (Pause)

        // A. KİLİTLENME YOKSA (Serbest Kamera)
        if (currentLockOnTarget == null)
        {
            lookAngle += (mouseXInput * lookSpeed) / delta;
            pivotAngle -= (mouseYInput * pivotSpeed) / delta;
            
            // Dikey açıyı sınırla (Tavana veya yere girmesin)
            pivotAngle = Mathf.Clamp(pivotAngle, minPivot, maxPivot);

            Vector3 rotation = Vector3.zero;
            rotation.y = lookAngle;
            Quaternion targetRotation = Quaternion.Euler(rotation);
            myTransform.rotation = targetRotation; // Yatay dönüş

            rotation = Vector3.zero;
            rotation.x = pivotAngle;
            targetRotation = Quaternion.Euler(rotation);
            cameraPivotTransform.localRotation = targetRotation; // Dikey dönüş (Pivot)
        }
        // B. KİLİTLENME VARSA (Locked-On)
        else
        {
            float distanceFromTarget = Vector3.Distance(targetTransform.position, currentLockOnTarget.position);

            Vector3 rotation = Vector3.zero;
            Vector3 dir = currentLockOnTarget.position - myTransform.position;
            dir.Normalize();
            dir.y = 0;

            // Kamerayı hedefe yavaşça çevir
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            myTransform.rotation = Quaternion.Slerp(myTransform.rotation, targetRotation, delta * 9);

            // Dinamik Yükseklik: Çok yaklaşınca kamera biraz aşağı insin (Kuş bakışı olmasın)
            float t = Mathf.Clamp01(distanceFromTarget / 3.0f); 
            float dynamicHeight = Mathf.Lerp(0.5f, 1.4f, t);

            Vector3 lockOnOffset = new Vector3(0, dynamicHeight, 0); 
            dir = (currentLockOnTarget.position + lockOnOffset) - cameraPivotTransform.position;
            dir.Normalize();

            targetRotation = Quaternion.LookRotation(dir);
            Vector3 euler = targetRotation.eulerAngles;
            euler.y = 0;
            
            // Açı düzeltmeleri (360 -> -180 dönüşümü)
            if (euler.x > 180) euler.x -= 360; 
            euler.x = Mathf.Clamp(euler.x, -20, 25); // Kilitliyken çok yukarı/aşağı gitmesin

            // Pivot'u ayarla
            Vector3 currentEuler = cameraPivotTransform.localEulerAngles;
            if (currentEuler.x > 180) currentEuler.x -= 360;
            float smoothX = Mathf.Lerp(currentEuler.x, euler.x, delta * 9);
            cameraPivotTransform.localEulerAngles = new Vector3(smoothX, 0, 0);

            // Pivot fiziksel pozisyonunu sabitle (Titremeyi önler)
            Vector3 localPos = cameraPivotTransform.localPosition;
            localPos.y = 1.8f; 
            cameraPivotTransform.localPosition = localPos;
            
            // Kilit kalkınca kamera saçma bir yere bakmasın diye açıları eşitle
            lookAngle = myTransform.eulerAngles.y;
            pivotAngle = cameraPivotTransform.localEulerAngles.x; 
        }
    }

    private Vector3 shakeOffset = Vector3.zero; // Sarsıntı vektörü

    // Kameranın duvarların içinden geçmesini önleyen sistem
    public void HandleCameraCollisions(float delta)
    {
        targetPosition = defaultPosition;
        RaycastHit hit;
        Vector3 direction = cameraTransform.position - cameraPivotTransform.position;
        direction.Normalize();

        // Kameradan geriye doğru bir küre fırlat, duvara çarparsa mesafeyi kısalt
        if (Physics.SphereCast(cameraPivotTransform.position, cameraCollisionRadius, direction, out hit, Mathf.Abs(targetPosition), collisionLayers))
        {
            float dis = Vector3.Distance(cameraPivotTransform.position, hit.point);
            targetPosition = -(dis - cameraCollisionOffset);
        }

        // Çok fazla yaklaşırsa minimum mesafede tut
        if (Mathf.Abs(targetPosition) < minimumCollisionOffset)
        {
            targetPosition = -minimumCollisionOffset;
        }

        // Yumuşak geçişle kamerayı yeni pozisyona taşı
        cameraTransformPosition.z = Mathf.Lerp(cameraTransform.localPosition.z, targetPosition, delta / 0.2f);
        
        // Sarsıntıyı (shakeOffset) da ekle
        cameraTransform.localPosition = cameraTransformPosition + shakeOffset;
    }

    // --- KAMERA SARSINTI SİSTEMİ ---
    public void ShakeCamera(float duration, float magnitude)
    {
        StopAllCoroutines(); 
        StartCoroutine(Shake(duration, magnitude));
    }

    private System.Collections.IEnumerator Shake(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float percentComplete = elapsed / duration;
            
            // Damper: Sarsıntı zamanla azalarak bitsin (Doğallık sağlar)
            float damper = 1.0f - percentComplete;

            // Rastgele X ve Y değerleri üret
            float x = Random.Range(-1f, 1f) * magnitude * damper;
            float y = Random.Range(-1f, 1f) * magnitude * damper;
            
            shakeOffset = new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero; // Sarsıntı bitince sıfırla
    }
}
