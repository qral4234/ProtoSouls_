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
    [Tooltip("İyileşme tuşuna basıldı mı? (Q).")]
    public bool heal_Input;
    
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

    // Yeni Input Sistemi referansı (Generated C# class)
    PlayerControls inputActions;

    // Script aktif olduğunda (OnEnable), input sistemini kurar
    public void OnEnable()
    {
        if (inputActions == null)
        {
            inputActions = new PlayerControls();

            // Hareket (WASD) okuma
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

            // Kamera (Fare) okuma
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

    // Script pasif olduğunda input sistemini de kapatır
    private void OnDisable()
    {
        inputActions.Disable();
    }

    // Her karede çalışarak anlık tuş vuruşlarını dinler
    public void TickInput(float delta)
    {
        // 0-1 arası hareket büyüklüğünü hesapla
        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");

        // Shift tuşu kontrolü (Koşma/Yuvarlanma)
        b_Input = Input.GetKey(KeyCode.LeftShift);

        // Shift basılı tutuluyorsa (Koşma)
        if (b_Input)
        {
            rollInputTimer += delta;
            // 0.22 saniyeden uzun basılırsa koşma olarak algıla
            if (rollInputTimer > 0.22f)
            {
                sprintFlag = true;
            }
        }
        else
        {
            // Tuş bırakıldıysa koşmayı durdur
            if (sprintFlag)
            {
                sprintFlag = false;
            }
            rollInputTimer = 0;
        }

        // Space tuşu ile Yuvarlanma
        if (Input.GetKeyDown(KeyCode.Space))
        {
             rollFlag = true;
        }

        // Fare Tıklamaları
        rb_Input = Input.GetMouseButtonDown(0); // Sol Tık (Saldırı)
        blockingInput = Input.GetMouseButton(1); // Sağ Tık (Blok)
        lockOn_Input = Input.GetMouseButtonDown(2); // Orta Tuş (Kilitlenme)
        
        // Klavye Kısayolları
        heal_Input = Input.GetKeyDown(KeyCode.Q); // Can Basma
    }
}
