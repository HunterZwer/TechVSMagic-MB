using UnityEngine;
using UnityEngine.AI;

public class UnitAttackState : StateMachineBehaviour
{
    private NavMeshAgent agent;
    private RangeAttackController rangeAttackController;
    private MeleeAttackController meleeAttackController;
    public float stopattackingDistance = 1.2f;

    public float attackRate = 1f;
    private float attackTimer;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent = animator.GetComponent<NavMeshAgent>();
        rangeAttackController = animator.GetComponent<RangeAttackController>();
        meleeAttackController = animator.GetComponent<MeleeAttackController>();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Transform targetToAttack = rangeAttackController != null ? rangeAttackController.targetToAttack : meleeAttackController.targetToAttack;

        if (targetToAttack != null && animator.transform.GetComponent<UnitMovement>().isCommandToMove == false)
        {
            LookAtTarget();

            if (attackTimer <= 0)
            {
                Attack();
                attackTimer = 1f / attackRate;
            }
            else
            {
                attackTimer -= Time.deltaTime;
            }

            float distanceFromTarget = Vector3.Distance(targetToAttack.position, animator.transform.position);
            if (distanceFromTarget > stopattackingDistance || targetToAttack == null)
            {
                agent.SetDestination(animator.transform.position);
                animator.SetBool("isAttacking", false);
            }
        }
        else
        {
            animator.SetBool("isAttacking", false);
        }
    }

    private void Attack()
    {
        if (rangeAttackController != null)
        {
            rangeAttackController.Attack();
        }
        else if (meleeAttackController != null)
        {
            // Assuming the MeleeAttackController has an Attack method
            meleeAttackController.Attack();
        }
    }

    private void LookAtTarget()
    {
        Transform targetToAttack = rangeAttackController != null ? rangeAttackController.targetToAttack : meleeAttackController.targetToAttack;
        Vector3 direction = targetToAttack.position - agent.transform.position;
        agent.transform.rotation = Quaternion.LookRotation(direction);

        var yRotation = agent.transform.eulerAngles.y;
        agent.transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Cleanup if needed
    }
}