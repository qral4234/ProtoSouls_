using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public float horizontal;
    public float vertical;
    public float moveAmount;
    public float mouseX;
    public float mouseY;
    public bool b_Input;
    public bool rb_Input;
    public bool blockingInput;
    public bool rollFlag;
    public bool sprintFlag;
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
        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");

        b_Input = Input.GetKey(KeyCode.LeftShift);

        if (b_Input)
        {
            rollInputTimer += delta;
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
            if (rollInputTimer < 0.22f)
            {
                sprintFlag = false;
                rollFlag = true;
            }
            
            rollInputTimer = 0;
        }

        rb_Input = Input.GetMouseButtonDown(0);
        blockingInput = Input.GetMouseButton(1);
    }
}
