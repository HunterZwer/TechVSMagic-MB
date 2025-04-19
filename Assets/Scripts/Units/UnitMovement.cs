using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class UnitMovement : MonoBehaviour
{
    private Camera _cam;
    public NavMeshAgent agent;
    private Animator _animator;
    private Transform _transform;
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    private UnitStats unitStats;
    private Unit _unit;
    private readonly float _rotationSpeed = 1500f;

    [Header("Upgrade Levels")]
    private int _speedUprgadeLevel = 0;
    private int _rotationUprgadeLevel = 0;

    public LayerMask ground;
    public bool isCommandToMove;
    private float _baseSpeed = 3.5f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        _unit = GetComponent<Unit>();
        _transform = transform;
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        _cam = Camera.main;
        unitStats = JsonLoader.LoadUnitStats(_unit.unitClass, _unit.IsPlayer);
        if (Upgrader.Instance is not null)
        {
            _speedUprgadeLevel = Upgrader.Instance.speedUpgradeLevel;
            _rotationUprgadeLevel = Upgrader.Instance.speedUpgradeLevel;
        }
        agent.speed *= unitStats.SpeedMultiplier[_speedUprgadeLevel];
        agent.angularSpeed *= unitStats.RotationMultiplier[_rotationUprgadeLevel] * _rotationSpeed;
        
        
    }
    
    public virtual void ApplySpeedeUpgrade()
    {
        agent.speed =  _baseSpeed * unitStats.DamageMultiplier[Upgrader.Instance.speedUpgradeLevel];

    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            List<GameObject> selectedUnits = UnitSelectionManager.Instance.unitSelected;

            if (selectedUnits.Count == 0) return;

            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ground))
            {
                AssignFormationMovement(selectedUnits, hit.point);
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

    private void AssignFormationMovement(List<GameObject> units, Vector3 centerPoint)
    {
        int unitCount = units.Count;
        int rowCount = Mathf.CeilToInt(Mathf.Sqrt(unitCount)); // Number of rows
        int columnCount = Mathf.CeilToInt((float)unitCount / rowCount); // Number of columns
        float spacing = 0.5f; // Distance between units

        int row = 0;
        int col = 0;

        foreach (GameObject unit in units)
        {
            if (unit.TryGetComponent(out NavMeshAgent unitAgent))
            {
                Vector3 offset = new Vector3(col * spacing, 0, row * spacing) - 
                                 new Vector3((columnCount - 1) * spacing / 2, 0, (rowCount - 1) * spacing / 2);
                Vector3 targetPosition = centerPoint + offset;
                unitAgent.SetDestination(targetPosition);
                unit.GetComponent<UnitMovement>().isCommandToMove = true;
                unit.GetComponent<Animator>().SetBool(IsMoving, true);
            }

            col++;
            if (col >= columnCount)
            {
                col = 0;
                row++;
            }
        }
    }
}
