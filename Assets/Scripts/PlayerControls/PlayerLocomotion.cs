using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    PlayerManager playerManager;
    InputHandler inputHandler;
    public Rigidbody playerRigidbody;
    AnimatorHandler animatorHandler;
    Transform cameraObject;

    [Header("Movement Stats")]
    public float movementSpeed = 5;
    public float sprintSpeed = 7;
    public float rotationSpeed = 10;

    [Header("Falling & Landing")]
    public float inAirTimer;
    public float leapingVelocity = 3f;
    public float fallingVelocity = 33f;
    public float rayCastHeightOffset = 0.5f;
    public LayerMask groundLayer;

    public bool isSprinting;
    public bool isGrounded;

    Vector3 moveDirection;

    void Start()
    {
        playerManager = GetComponent<PlayerManager>();
        playerRigidbody = GetComponent<Rigidbody>();
        inputHandler = GetComponent<InputHandler>();
        animatorHandler = GetComponentInChildren<AnimatorHandler>();
        cameraObject = Camera.main.transform;

        isGrounded = true;
        // Default ground layer to "Default" (1) if not set, usually safest to let user set it in inspector
        // But for code safety let's assume everything except player
        if (groundLayer == 0)
        {
            groundLayer = 1; 
        }
    }

    public void HandleRollingAndSprinting(float delta)
    {
        if (animatorHandler.anim.GetBool("isInteracting"))
            return;

        if (inputHandler.rollFlag)
        {
            moveDirection = cameraObject.forward * inputHandler.vertical;
            moveDirection += cameraObject.right * inputHandler.horizontal;

            if (inputHandler.moveAmount > 0)
            {
                animatorHandler.PlayTargetAnimation("Rolling", true);
                moveDirection.y = 0;
                Quaternion rollRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = rollRotation;
            }
            else
            {
                animatorHandler.PlayTargetAnimation("Rolling", true);
            }
            
            inputHandler.rollFlag = false;
        }

        if (inputHandler.sprintFlag && inputHandler.moveAmount > 0.5f)
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }
    }

    public void HandleAllMovement(float delta)
    {
        HandleFalling(delta, moveDirection);

        if (inputHandler.rollFlag || animatorHandler.anim.GetBool("isInteracting"))
            return;

        HandleMovement(delta);
        HandleRotation(delta);
    }

    private void HandleMovement(float delta)
    {
        if (playerManager.isBlocking)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            return;
        }

        if (playerManager.isDead)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            return;
        }

        if (inputHandler.rollFlag)
            return;

        if (isGrounded == false)
            return;

        moveDirection = cameraObject.forward * inputHandler.vertical;
        moveDirection += cameraObject.right * inputHandler.horizontal;

        moveDirection.Normalize();
        moveDirection.y = 0;

        float speed = movementSpeed;
        if (isSprinting)
        {
            speed = sprintSpeed;
        }
        moveDirection *= speed;

        Vector3 movementVelocity = moveDirection;
        playerRigidbody.linearVelocity = new Vector3(movementVelocity.x, playerRigidbody.linearVelocity.y, movementVelocity.z);
    }

    public void HandleRotation(float delta)
    {
        if (playerManager.isBlocking)
            return;

        if (playerManager.isDead)
            return;
            
        if (animatorHandler.anim.GetBool("isInteracting"))
            return;

        Vector3 targetDir = Vector3.zero;
        float moveOverride = inputHandler.moveAmount;

        targetDir = cameraObject.forward * inputHandler.vertical;
        targetDir += cameraObject.right * inputHandler.horizontal;

        targetDir.Normalize();
        targetDir.y = 0;

        if (targetDir == Vector3.zero)
            targetDir = transform.forward;

        float rs = rotationSpeed;

        Quaternion tr = Quaternion.LookRotation(targetDir);
        Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, rs * delta);

        transform.rotation = targetRotation;
    }

    public void HandleFalling(float delta, Vector3 moveDirection)
    {
        isGrounded = false;
        RaycastHit hit;
        Vector3 origin = transform.position;
        origin.y += rayCastHeightOffset;

        if (Physics.SphereCast(origin, 0.2f, Vector3.down, out hit, 1f, groundLayer))
        {
            isGrounded = true;
        }

        if (!isGrounded)
        {
            animatorHandler.anim.SetBool("isGrounded", false);
            animatorHandler.PlayTargetAnimation("Falling", true);

            inAirTimer += delta;
            
            playerRigidbody.AddForce(transform.forward * leapingVelocity);
            
            Vector3 vel = playerRigidbody.linearVelocity;
            vel.y -= fallingVelocity * delta;
            playerRigidbody.linearVelocity = vel;
        }
        else
        {
            animatorHandler.anim.SetBool("isGrounded", true);
            inAirTimer = 0;
            
            if (isGrounded && animatorHandler.anim.GetBool("isGrounded") == false)
            {
                animatorHandler.PlayTargetAnimation("Land", true);
            }
        }
    }
}
