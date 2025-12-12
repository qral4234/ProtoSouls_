using UnityEngine;
using UnityEngine.AI;

public class EnemyManager : MonoBehaviour
{
    EnemyStats enemyStats;
    NavMeshAgent navMeshAgent;
    Animator animator;
    public Transform currentTarget;
    public float distanceFromTarget;
    public float stoppingDistance = 1.5f;
    public float rotationSpeed = 15f;
    public bool isPreformingAction;

    [Header("Combat Settings")]
    public float currentRecoveryTime = 0;
    public float attackRange = 1.5f;

    public DamageCollider rightHandDamageCollider;
    public DamageCollider leftHandDamageCollider;

    public PlayerManager targetPlayerManager;

    private void Awake()
    {
        enemyStats = GetComponent<EnemyStats>();
        navMeshAgent = GetComponentInChildren<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            currentTarget = player.transform;
            targetPlayerManager = player.GetComponent<PlayerManager>();
        }
        
        // Optionally find colliders if they are not assigned in inspector, but user implies public assignment.
        // For now relying on Inspector assignment as per typical Unity workflow for "public" fields requested.
    }

    private void Update()
    {
        HandleRecoveryTimer();
        HandleCurrentAction();
    }

    private void HandleRecoveryTimer()
    {
        if (currentRecoveryTime > 0)
        {
            currentRecoveryTime -= Time.deltaTime;
        }

        if (isPreformingAction)
        {
            if (currentRecoveryTime < 1.0f)
            {
                isPreformingAction = false;
            }
        }
    }

    private void HandleCurrentAction()
    {
        if (enemyStats.currentHealth <= 0)
            return;

        if (currentTarget == null)
            return;

        if (targetPlayerManager != null && targetPlayerManager.isDead)
        {
             navMeshAgent.enabled = false;
             isPreformingAction = true;
             animator.SetBool("isWinner", true);
             return;
        }

        distanceFromTarget = Vector3.Distance(currentTarget.position, transform.position);

        if (distanceFromTarget > stoppingDistance)
        {
            if (isPreformingAction)
            {
                navMeshAgent.enabled = false;
            }
            else
            {
                navMeshAgent.enabled = true;
                navMeshAgent.SetDestination(currentTarget.position);
                animator.SetFloat("Vertical", 1, 0.1f, Time.deltaTime);
            }
        }
        else
        {
            navMeshAgent.enabled = false;
            animator.SetFloat("Vertical", 0, 0.1f, Time.deltaTime);
            RotateTowardsTarget();

            if (!isPreformingAction)
            {
                AttackTarget();
            }
        }
    }

    private void AttackTarget()
    {
        if (currentRecoveryTime > 0)
            return;

        isPreformingAction = true;
        currentRecoveryTime = 2.5f;

        int randomAttack = Random.Range(0, 2);
        animator.SetInteger("AttackIndex", randomAttack);
        animator.SetTrigger("isAttacking");

        if (randomAttack == 0)
        {
            rightHandDamageCollider.currentHitAnimation = "GetHit_01";
            leftHandDamageCollider.currentHitAnimation = "GetHit_01";
        }
        else if (randomAttack == 1)
        {
            rightHandDamageCollider.currentHitAnimation = "GetHit_02";
            leftHandDamageCollider.currentHitAnimation = "GetHit_02";
        }
    }

    private void RotateTowardsTarget()
    {
        Vector3 direction = currentTarget.position - transform.position;
        direction.y = 0;
        
        if (direction == Vector3.zero)
            direction = transform.forward;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    #region Animation Events
    public void OpenRightDamageCollider()
    {
        rightHandDamageCollider.EnableDamageCollider();
    }

    public void CloseRightDamageCollider()
    {
        rightHandDamageCollider.DisableDamageCollider();
    }

    public void OpenLeftDamageCollider()
    {
        Debug.Log("SOL EL AÇILDI!"); 
        leftHandDamageCollider.EnableDamageCollider();
    }

    public void CloseLeftDamageCollider()
    {
        leftHandDamageCollider.DisableDamageCollider();
    }
    #endregion
}
