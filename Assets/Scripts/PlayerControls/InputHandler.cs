using UnityEngine;

public class InputHandler : MonoBehaviour
{
    [Header("Hareket Girdileri")]
    [Tooltip("Yatay eksen girdisi (A/D veya Sol Analog).")]
    public float horizontal;
    [Tooltip("Dikey eksen girdisi (W/S veya Sol Analog).")]
    public float vertical;
    [Tooltip("Toplam hareket miktarı (0 ile 1 arası).")]
    public float moveAmount;
    
    [Header("Kamera Girdileri")]
    [Tooltip("Fare X ekseni (Sağ/Sol bakış).")]
    public float mouseX;
    [Tooltip("Fare Y ekseni (Yukarı/Aşağı bakış).")]
    public float mouseY;
    
    [Header("Aksiyon Girdileri")]
    [Tooltip("Sprint/Yuvarlanma tuşuna basılıyor mu? (Genellikle Shift veya B).")]
    public bool b_Input;
    [Tooltip("Hafif saldırı tuşuna basıldı mı? (Sol Tık).")]
    public bool rb_Input;
    [Tooltip("Bloklama tuşuna basılıyor mu? (Sağ Tık).")]
    public bool blockingInput;
    
    [Header("Durum İşaretçileri (Flags)")]
    [Tooltip("Yuvarlanma tetiklendi.")]
    public bool rollFlag;
    [Tooltip("Koşma tetiklendi.")]
    public bool sprintFlag;
    [Tooltip("Kilitlenme tuşuna basıldı (Orta Fare).")]
    public bool lockOn_Input;
    
    [Header("Zamanlayıcılar")]
    [Tooltip("Yuvarlanma ile koşma arasındaki farkı anlamak için zamanlayıcı.")]
    public float rollInputTimer;

    PlayerControls inputActions;

    public void OnEnable()
    {
        if (inputActions == null)
        {
            inputActions = new PlayerControls();

            inputActions.Player.Move.performed += inputActions => 
            {
                Vector2 input = inputActions.ReadValue<Vector2>();
                horizontal = input.x;
                vertical = input.y;
            };
            inputActions.Player.Move.canceled += inputActions => 
            {
                horizontal = 0;
                vertical = 0;
            };

            inputActions.Player.Look.performed += inputActions => 
            {
                Vector2 input = inputActions.ReadValue<Vector2>();
                mouseX = input.x;
                mouseY = input.y;
            };
            inputActions.Player.Look.canceled += inputActions => 
            {
                mouseX = 0;
                mouseY = 0;
            };
        }

        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    public void TickInput(float delta)
    {
        // Hareketi oku ve işle
        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");

        // Sprint ve Roll (Shift) mantığı
        b_Input = Input.GetKey(KeyCode.LeftShift);

        if (b_Input)
        {
            rollInputTimer += delta;
            // Tuşa basılı tutuluyorsa koşma moduna geç
            if (rollInputTimer > 0.22f)
            {
                sprintFlag = true;
            }
        }
        else
        {
            if (sprintFlag)
                sprintFlag = false;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            // Eğer kısa basılıp çekildiyse yuvarlan
            if (rollInputTimer < 0.22f)
            {
                sprintFlag = false;
                rollFlag = true;
            }
            
            rollInputTimer = 0;
        }

        // Saldırı ve Blok girdileri
        rb_Input = Input.GetMouseButtonDown(0);
        blockingInput = Input.GetMouseButton(1);
        lockOn_Input = Input.GetMouseButtonDown(2); // Orta tuş ile kilitlenme
    }
}
