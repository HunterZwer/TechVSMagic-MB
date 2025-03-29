using UnityEngine;
using UnityEngine.AI;

public class UnitAttackState : StateMachineBehaviour
{
    // Configuration
    public float stopattackingDistance = 1.2f;
    public float attackRate = 1f;
    
    // Cached state
    private float _attackTimer;
    private float _attackInterval;
    private Transform _cachedTransform;
    private UnitMovement _unitMovement;
    
    // Component references
    private NavMeshAgent _agent;
    private RangeAttackController _rangeAttackController;
    private MeleeAttackController _meleeAttackController;
    
    // Cached for performance
    private static readonly int IsAttacking = Animator.StringToHash("isAttacking");
    private Vector3 _directionToTarget = Vector3.zero;
    private float _sqrStopAttackingDistance;
    
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Cache all components needed
        _cachedTransform = animator.transform;
        _agent = animator.GetComponent<NavMeshAgent>();
        _rangeAttackController = animator.GetComponent<RangeAttackController>();
        _meleeAttackController = animator.GetComponent<MeleeAttackController>();
        _unitMovement = animator.GetComponent<UnitMovement>();
        
        // Precalculate values
        _attackInterval = 1f / attackRate;
        _attackTimer = 0f;
        _sqrStopAttackingDistance = stopattackingDistance * stopattackingDistance;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Get current target (prefer range attack target if available)
        Transform targetToAttack = _rangeAttackController != null ? 
            _rangeAttackController.targetToAttack : 
            _meleeAttackController?.targetToAttack;

        // Check if we have a valid target and aren't commanded to move elsewhere
        if (targetToAttack != null && (_unitMovement == null || !_unitMovement.isCommandToMove))
        {
            // Look at target
            LookAtTarget(targetToAttack);

            // Handle attack timing
            if (_attackTimer <= 0)
            {
                Attack();
                _attackTimer = _attackInterval;
            }
            else
            {
                _attackTimer -= Time.deltaTime;
            }

            // Check if target is still in range using square distance (more efficient)
            _directionToTarget = targetToAttack.position - _cachedTransform.position;
            float sqrDistanceFromTarget = _directionToTarget.sqrMagnitude;
            
            if (sqrDistanceFromTarget > _sqrStopAttackingDistance)
            {
                // Target out of range, stop attacking
                _agent.SetDestination(_cachedTransform.position);
                animator.SetBool(IsAttacking, false);
            }
        }
        else
        {
            // No target or movement command, stop attacking
            animator.SetBool(IsAttacking, false);
        }
    }

    private void Attack()
    {
        // Try range attack first, then melee
        if (_rangeAttackController != null)
        {
            _rangeAttackController.Attack();
        }
        else if (_meleeAttackController != null)
        {
            _meleeAttackController.Attack();
        }
    }

    private void LookAtTarget(Transform target)
    {
        // Only calculate direction once
        _directionToTarget = target.position - _cachedTransform.position;
        _directionToTarget.y = 0; // Keep rotation on the horizontal plane
        
        // Avoid computing rotation if direction is too small
        if (_directionToTarget.sqrMagnitude > 0.001f)
        {
            // Set rotation directly - faster than slerp for instant rotation
            _cachedTransform.rotation = Quaternion.LookRotation(_directionToTarget);
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // No cleanup needed
    }
}