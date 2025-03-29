using UnityEngine;

public class UnitIDLEState : StateMachineBehaviour
{
    // Cached component references
    private RangeAttackController _rangeAttackController;
    private MeleeAttackController _meleeAttackController;
    private Transform _cachedTransform;
    
    // Cache animator parameter hash for better performance
    private static readonly int IsFollowing = Animator.StringToHash("isFollowing");
    
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Cache transform to improve performance
        _cachedTransform = animator.transform;
        
        // Cache components to avoid GetComponent calls during updates
        _rangeAttackController = _cachedTransform.GetComponent<RangeAttackController>();
        _meleeAttackController = _cachedTransform.GetComponent<MeleeAttackController>();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Early exit if no attack controllers attached
        if (_rangeAttackController == null && _meleeAttackController == null)
            return;
            
        // Get the target from the appropriate controller (prefer range controller if available)
        Transform targetToAttack = _rangeAttackController != null ? 
            _rangeAttackController.targetToAttack : 
            _meleeAttackController?.targetToAttack;

        // If a target is found, transition to the follow state
        if (targetToAttack != null)
        {
            animator.SetBool(IsFollowing, true);
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // No cleanup needed
    }
}