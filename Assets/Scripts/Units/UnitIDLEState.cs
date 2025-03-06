using UnityEngine;

public class UnitIDLEState : StateMachineBehaviour
{
    private RangeAttackController rangeAttackController;
    private MeleeAttackController meleeAttackController;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        rangeAttackController = animator.transform.GetComponent<RangeAttackController>();
        meleeAttackController = animator.transform.GetComponent<MeleeAttackController>();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Check if either controller is assigned
        if (rangeAttackController != null || meleeAttackController != null)
        {
            // Get the target from the appropriate controller
            Transform targetToAttack = rangeAttackController != null ? rangeAttackController.targetToAttack : meleeAttackController.targetToAttack;

            // If a target is found, transition to the follow state
            if (targetToAttack != null)
            {
                animator.SetBool("isFollowing", true);
            }
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Cleanup if needed
    }
}