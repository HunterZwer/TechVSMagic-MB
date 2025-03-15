using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
public class UnitMovement : MonoBehaviour
{
    Camera cam;
    public NavMeshAgent agent;
    public LayerMask ground;
    public bool isCommandToMove;
    private Animator animator;
    private void Start()
    {
        cam = Camera.main;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            List<GameObject> selectedUnits = UnitSelectionManager.Instance.unitSelected;

            foreach (GameObject unit in selectedUnits)
            {
                if (unit == this.gameObject) 
                {
                    RaycastHit hit;
                    Ray ray = cam.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground))
                    {
                        isCommandToMove = true;
                        agent.SetDestination(hit.point);
                        animator.SetBool("isMoving", true);
                    }
                }
            }
        }

        if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance)
        {
            isCommandToMove = false;
            animator.SetBool("isMoving", false);
        }
        else
        {
            animator.SetBool("isMoving", true);
        }
    }
}