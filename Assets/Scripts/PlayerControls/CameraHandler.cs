using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    public static CameraHandler singleton;

    [Header("Hedef Takibi")]
    [Tooltip("Kameranın takip edeceği hedef (Genellikle Oyuncu).")]
    public Transform targetTransform;
    [Tooltip("Kamera objesinin transformu.")]
    public Transform cameraTransform;
    [Tooltip("Kamera pivot noktası (Yükseklik ayarı için).")]
    public Transform cameraPivotTransform;
    
    private Transform myTransform;
    private Vector3 cameraTransformPosition;
    private LayerMask ignoreLayers;
    private Vector3 cameraFollowVelocity = Vector3.zero;

    [Header("Kamera Hareket Ayarları")]
    [Tooltip("Kameranın bakış hızı (Sağ/Sol dönüş hassasiyeti).")]
    public float lookSpeed = 0.1f;
    [Tooltip("Kameranın hedefini takip etme hızı.")]
    public float followSpeed = 0.1f;
    [Tooltip("Kameranın dikey dönüş hızı (Yukarı/Aşağı).")]
    public float pivotSpeed = 0.03f;

    private float targetPosition;
    private float defaultPosition;
    private float lookAngle;
    private float pivotAngle;
    
    [Header("Kamera Sınırları")]
    [Tooltip("Kameranın aşağı bakabileceği maksimum açı.")]
    public float minPivot = -35;
    [Tooltip("Kameranın yukarı bakabileceği maksimum açı.")]
    public float maxPivot = 35;

    [Header("Kamera Çarpışma Ayarları")]
    [Tooltip("Kameranın duvarlara çarpmaması için kullanılan çarpışma yarıçapı.")]
    public float cameraCollisionRadius = 0.2f;
    [Tooltip("Duvardan ne kadar uzakta duracağı.")]
    public float cameraCollisionOffset = 0.2f;
    [Tooltip("Minimum çarpışma mesafesi.")]
    public float minimumCollisionOffset = 0.2f;
    [Tooltip("Hangi layerların kamerayı engelleyeceğini belirler.")]
    public LayerMask collisionLayers;

    [Header("Kilitlenme (Lock-On) Sistemi")]
    [Tooltip("Şu anda kilitlenilen hedef.")]
    public Transform currentLockOnTarget;

    private void Awake()
    {
        singleton = this;
        myTransform = transform;
        defaultPosition = cameraTransform.localPosition.z;
        // Oyuncu ve ilgili layerları görmezden gel
        ignoreLayers = ~(1 << 8 | 1 << 9 | 1 << 10);
        collisionLayers = ~(1 << 8 | 1 << 9 | 1 << 10); 
    }

    public void FollowTarget(float delta)
    {
        // Hedefi yumuşak bir şekilde takip et
        Vector3 targetPosition = Vector3.Lerp(myTransform.position, targetTransform.position, delta / followSpeed);
        myTransform.position = targetPosition;
    }

    public void HandleLockOn()
    {
        InputHandler inputHandler = FindFirstObjectByType<InputHandler>();

        if (inputHandler.lockOn_Input)
        {
            // Eğer zaten kilitlenmişse kilidi kaldır, değilse en yakın hedefi bul
            if (currentLockOnTarget == null)
            {
                currentLockOnTarget = GetNearestTarget();
            }
            else
            {
                currentLockOnTarget = null;
            }
        }
    }

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
                
                // Görüş açısında mı kontrolü
                Vector3 direction = hit.transform.position - targetTransform.position;
                float angle = Vector3.Angle(cameraTransform.forward, direction);
                
                if (angle < 50 && distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestTarget = hit.transform;
                }
            }
        }
        return nearestTarget;
    }

    public void HandleCameraRotation(float delta, float mouseXInput, float mouseYInput)
    {
        if (currentLockOnTarget == null)
        {
            // Normal (Serbest) Kamera Modu
            lookAngle += (mouseXInput * lookSpeed) / delta;
            pivotAngle -= (mouseYInput * pivotSpeed) / delta;
            
            pivotAngle = Mathf.Clamp(pivotAngle, minPivot, maxPivot);

            Vector3 rotation = Vector3.zero;
            rotation.y = lookAngle;
            Quaternion targetRotation = Quaternion.Euler(rotation);
            myTransform.rotation = targetRotation;

            rotation = Vector3.zero;
            rotation.x = pivotAngle;
            targetRotation = Quaternion.Euler(rotation);
            cameraPivotTransform.localRotation = targetRotation;
        }
        else
        {
            // Kilitlenme (Locked-On) Kamera Modu
            Vector3 rotation = Vector3.zero;
            Vector3 dir = currentLockOnTarget.position - myTransform.position;
            dir.Normalize();
            dir.y = 0;

            Quaternion targetRotation = Quaternion.LookRotation(dir);
            myTransform.rotation = Quaternion.Slerp(myTransform.rotation, targetRotation, delta * 9);

            // Yakın Mesafe Mantığı (Offset ile hedefe bakma)
            Vector3 lockOnOffset = new Vector3(0, 1.4f, 0); // Göğüs hizasına odaklan
            dir = (currentLockOnTarget.position + lockOnOffset) - cameraPivotTransform.position;
            dir.Normalize();

            targetRotation = Quaternion.LookRotation(dir);
            Vector3 euler = targetRotation.eulerAngles;
            euler.y = 0;
            
            // Dikey açı (Pitch) sınırlaması: -30 ile 40 derece arası
            if (euler.x > 180) euler.x -= 360; 
            euler.x = Mathf.Clamp(euler.x, -30, 40);

            cameraPivotTransform.localEulerAngles = euler;

            // Yükseklik sınırlaması (Kameranın çok yukarı kalkmasını önle)
            Vector3 localPos = cameraPivotTransform.localPosition;
            localPos.y = Mathf.Clamp(localPos.y, 0, 2.0f); 
            
            // Normal moda dönerken kameranın zıplamasını önlemek için açıyı senkronize et
            lookAngle = myTransform.eulerAngles.y;
        }
    }

    public void HandleCameraCollisions(float delta)
    {
        targetPosition = defaultPosition;
        RaycastHit hit;
        Vector3 direction = cameraTransform.position - cameraPivotTransform.position;
        direction.Normalize();

        // Kamera ile oyuncu arasına engel giriyor mu kontrol et
        if (Physics.SphereCast(cameraPivotTransform.position, cameraCollisionRadius, direction, out hit, Mathf.Abs(targetPosition), collisionLayers))
        {
            float dis = Vector3.Distance(cameraPivotTransform.position, hit.point);
            targetPosition = -(dis - cameraCollisionOffset);
        }

        if (Mathf.Abs(targetPosition) < minimumCollisionOffset)
        {
            targetPosition = -minimumCollisionOffset;
        }

        cameraTransformPosition.z = Mathf.Lerp(cameraTransform.localPosition.z, targetPosition, delta / 0.2f);
        cameraTransform.localPosition = cameraTransformPosition;
    }
}
