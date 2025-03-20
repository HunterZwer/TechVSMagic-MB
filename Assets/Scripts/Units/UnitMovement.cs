using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class UnitMovement : MonoBehaviour
{
    // Cached references
    private Camera _cam;
    public NavMeshAgent agent; // Public as requested
    private Animator _animator;
    private Transform _transform;
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    private UnitStats unitStats;
    private Unit _unit;
    
    [Header("Upgrade Levels")]
    private int _SpeedUprgadeLevel = 0;
    
    // Public fields
    public LayerMask ground;
    public bool isCommandToMove;
    
    private void Awake()
    {
        _unit = GetComponent<Unit>();
        // Cache components in Awake
        _transform = transform;
        _animator = GetComponent<Animator>();
    }
    
    private void Start()
    {
        _cam = Camera.main;
        agent = GetComponent<NavMeshAgent>();
        unitStats = JsonLoader.LoadUnitStats(_unit.unitClass, _unit.IsPlayer);
        agent.speed = agent.speed * unitStats.SpeedMultiplier[_SpeedUprgadeLevel];
    }
    
    private void Update()
    {
        // Process mouse input
        if (Input.GetMouseButtonDown(1))
        {
            List<GameObject> selectedUnits = UnitSelectionManager.Instance.unitSelected;
            
            foreach (GameObject unit in selectedUnits)
            {
                if (unit == gameObject)
                {
                    Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
                    
                    if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ground))
                    {
                        isCommandToMove = true;
                        agent.SetDestination(hit.point);
                        _animator.SetBool(IsMoving, true);
                    }
                }
            }
        }
        
        if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance)
        {
            isCommandToMove = false;
            _animator.SetBool(IsMoving, false);
        }
        else
        {
            _animator.SetBool(IsMoving, true);
        }
    }
}