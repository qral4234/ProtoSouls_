using UnityEngine;

public class PlayerRageManager : MonoBehaviour
{
    InputHandler inputHandler;
    AnimatorHandler animatorHandler;

    [Header("UI")]
    public RageBar rageBar;

    [Header("Rage Stats")]
    public float maxRage = 100;
    public float currentRage = 0;
    public bool isRageActive = false;
    public float damageMultiplier = 1.5f;

    [Header("Penalty Stack")]
    public int usageStack = 0;
    // Stack 0: 100% Fill Rate (Normal)
    // Stack 1: 85% Fill Rate
    // Stack 2: 70% Fill Rate
    // Stack 3+: 55% Fill Rate (Max penalty)
    
    [Header("Cooldown")]
    public float cooldownTimer = 0;
    public float cooldownDuration = 15f;

    [Header("Drain Settings")]
    public float sprintDrainAmount = 5f;
    public float attackDrainAmount = 10f;
    // Attack drain is handled via external call from PlayerAttacker

    private void Awake()
    {
        inputHandler = GetComponent<InputHandler>();
        animatorHandler = GetComponentInChildren<AnimatorHandler>();
    }

    private void Start()
    {
        if (rageBar != null)
        {
            rageBar.SetMaxRage(maxRage);
            rageBar.SetCurrentRage(currentRage);
        }
    }

    private void Update()
    {
        float delta = Time.deltaTime;

        HandleCooldown(delta);
        HandleRageActivation();
        HandleSprintDrain(delta);
    }

    private void HandleCooldown(float delta)
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= delta;
        }
    }

    private void HandleRageActivation()
    {
        // Only activate if not active, cooldown is over, and rage is full
        if (!isRageActive && cooldownTimer <= 0 && currentRage >= maxRage)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                ActivateRage();
            }
        }
    }

    public void ActivateRage()
    {
        isRageActive = true;
        animatorHandler.PlayTargetAnimation("Rage_Activate", true);
    }

    public void DeactivateRage()
    {
        isRageActive = false;
        currentRage = 0;
        
        if (rageBar != null)
            rageBar.SetCurrentRage(0);

        usageStack++;
        cooldownTimer = cooldownDuration;
    }

    public void AddRage(float amount)
    {
        if (isRageActive) return;
        if (cooldownTimer > 0) return;

        // Calculate fill rate based on stack
        float fillMultiplier = 1.0f;
        if (usageStack == 1) fillMultiplier = 0.85f;
        else if (usageStack == 2) fillMultiplier = 0.70f;
        else if (usageStack >= 3) fillMultiplier = 0.55f;

        currentRage += amount * fillMultiplier;

        if (currentRage >= maxRage)
            currentRage = maxRage;

        if (rageBar != null)
            rageBar.SetCurrentRage(currentRage);
    }

    public void DrainRage(float amount)
    {
        if (!isRageActive) return;

        currentRage -= amount;

        if (currentRage <= 0)
        {
            currentRage = 0;
            DeactivateRage();
        }

        if (rageBar != null)
            rageBar.SetCurrentRage(currentRage);
    }

    private void HandleSprintDrain(float delta)
    {
        if (isRageActive && inputHandler.sprintFlag && inputHandler.moveAmount > 0.5f)
        {
            DrainRage(sprintDrainAmount * delta);
        }
    }
}
