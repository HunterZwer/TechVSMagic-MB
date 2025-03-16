using UnityEngine;
using UnityEngine.AI;

public class UnitFollowState : StateMachineBehaviour
{
    // Configuration
    public float attackingDistance = 1f;
    
    // Cached references
    private RangeAttackController _rangeAttackController;
    private MeleeAttackController _meleeAttackController;
    private NavMeshAgent _agent;
    private Transform _cachedTransform;
    private UnitMovement _unitMovement;
    
    // Performance optimization
    private float _sqrAttackingDistance;
    private static readonly int IsFollowing = Animator.StringToHash("isFollowing");
    private static readonly int IsAttacking = Animator.StringToHash("isAttacking");
    
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Cache components for better performance
        _cachedTransform = animator.transform;
        _rangeAttackController = _cachedTransform.GetComponent<RangeAttackController>();
        _meleeAttackController = _cachedTransform.GetComponent<MeleeAttackController>();
        _agent = _cachedTransform.GetComponent<NavMeshAgent>();
        _unitMovement = _cachedTransform.GetComponent<UnitMovement>();
        
        // Precalculate squared distance for more efficient comparison
        _sqrAttackingDistance = attackingDistance * attackingDistance;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Get current target (prefer range controller if available)
        Transform targetToAttack = _rangeAttackController != null ? 
            _rangeAttackController.targetToAttack : 
            _meleeAttackController?.targetToAttack;

        // No target, exit following state
        if (targetToAttack == null)
        {
            animator.SetBool(IsFollowing, false);
            return;
        }
        
        // Only proceed if not commanded to move elsewhere
        if (_unitMovement != null && !_unitMovement.isCommandToMove)
        {
            // Update destination to follow target
            _agent.SetDestination(targetToAttack.position);
            
            // Calculate direction for look at
            Vector3 direction = targetToAttack.position - _cachedTransform.position;
            direction.y = 0; // Keep rotation on horizontal plane
            
            // Only rotate if direction is valid
            if (direction.sqrMagnitude > 0.001f)
            {
                _cachedTransform.rotation = Quaternion.LookRotation(direction);
            }
            
            // Check distance to target using squared magnitude (more efficient than Vector3.Distance)
            float sqrDistanceFromTarget = direction.sqrMagnitude;
            
            // If close enough to target, switch to attack state
            if (sqrDistanceFromTarget < _sqrAttackingDistance)
            {
                // Stop moving when in attack range
                _agent.SetDestination(_cachedTransform.position);
                animator.SetBool(IsAttacking, true);
            }
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // No cleanup needed
    }
}