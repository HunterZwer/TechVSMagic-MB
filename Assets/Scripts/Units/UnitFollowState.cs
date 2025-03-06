using UnityEngine;
using UnityEngine.AI;

public class UnitFollowState : StateMachineBehaviour
{
    private RangeAttackController rangeAttackController;
    private MeleeAttackController meleeAttackController;
    private NavMeshAgent agent;
    public float attackingDistance = 1f;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        rangeAttackController = animator.transform.GetComponent<RangeAttackController>();
        meleeAttackController = animator.transform.GetComponent<MeleeAttackController>();
        agent = animator.transform.GetComponent<NavMeshAgent>();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Transform targetToAttack = rangeAttackController != null ? rangeAttackController.targetToAttack : meleeAttackController.targetToAttack;

        if (targetToAttack == null)
        {
            animator.SetBool("isFollowing", false);
        }
        else
        {
            if (animator.transform.GetComponent<UnitMovement>().isCommandToMove == false)
            {
                agent.SetDestination(targetToAttack.position);
                animator.transform.LookAt(targetToAttack);
                float distanceFromTarget = Vector3.Distance(targetToAttack.position, animator.transform.position);
                if (distanceFromTarget < attackingDistance)
                {
                    agent.SetDestination(animator.transform.position);
                    animator.SetBool("isAttacking", true);
                }
            }
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Cleanup if needed
    }
}