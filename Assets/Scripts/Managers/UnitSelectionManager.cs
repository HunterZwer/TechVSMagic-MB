using UnityEngine;
using System.Collections.Generic;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; private set; }

    public List<GameObject> allUnitSelected = new List<GameObject>();
    public List<GameObject> unitSelected = new List<GameObject>();
    private List<(GameObject unit, Component attackComponent)> selectedUnitsWithAttack = new List<(GameObject, Component)>();

    public LayerMask clickable;
    public LayerMask ground;
    public LayerMask attackable;
    public GameObject groundMarker;
    public bool attackCursorVisible;

    private Camera cam;
    private bool _hasOffensiveUnits;

    public delegate void SelectionChanged();
    public event SelectionChanged onSelectionChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        HandleLeftClick();
        HandleRightClick();
        UpdateAttackCursor();
    }

    private void HandleLeftClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, clickable))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    MultiSelect(hit.collider.gameObject);
                }
                else
                {
                    SelectByClicking(hit.collider.gameObject);
                }
            }
            else if (!Input.GetKey(KeyCode.LeftShift))
            {
                DeselectAll();
            }
        }
    }

    private void HandleRightClick()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, clickable))
            {
                groundMarker.transform.position = hit.point;
                groundMarker.SetActive(true);
            }

            if (_hasOffensiveUnits && Physics.Raycast(ray, out hit, Mathf.Infinity, attackable))
            {
                Transform target = hit.transform;
                foreach (var (unit, attackComponent) in selectedUnitsWithAttack)
                {
                    if (attackComponent is MeleeAttackController melee)
                    {
                        melee.targetToAttack = target;
                    }
                    else if (attackComponent is RangeAttackController ranged)
                    {
                        ranged.targetToAttack = target;
                    }
                }
            }
        }
    }

    private void UpdateAttackCursor()
    {
        if (_hasOffensiveUnits)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            attackCursorVisible = Physics.Raycast(ray, out _, Mathf.Infinity, attackable);
        }
        else
        {
            attackCursorVisible = false;
        }
    }

    private void SelectByClicking(GameObject unit)
    {
        DeselectAll();
        unitSelected.Add(unit);
        SelectUnit(unit, true);
        AddUnitToAttackList(unit);
        onSelectionChanged?.Invoke();
    }

    private void MultiSelect(GameObject unit)
    {
        if (!unitSelected.Contains(unit))
        {
            unitSelected.Add(unit);
            SelectUnit(unit, true);
            AddUnitToAttackList(unit);
        }
        else
        {
            SelectUnit(unit, false);
            unitSelected.Remove(unit);
            RemoveUnitFromAttackList(unit);
        }
        onSelectionChanged?.Invoke();
    }

    private void AddUnitToAttackList(GameObject unit)
    {
        MeleeAttackController melee = unit.GetComponent<MeleeAttackController>();
        RangeAttackController ranged = unit.GetComponent<RangeAttackController>();
        
        if (melee != null || ranged != null)
        {
            selectedUnitsWithAttack.Add((unit, melee != null ? (Component)melee : (Component)ranged));
            _hasOffensiveUnits = true;
        }
    }

    private void RemoveUnitFromAttackList(GameObject unit)
    {
        selectedUnitsWithAttack.RemoveAll(u => u.unit == unit);
        _hasOffensiveUnits = selectedUnitsWithAttack.Count > 0;
    }

    public void DeselectAll()
    {
        unitSelected.RemoveAll(u => u == null);
        selectedUnitsWithAttack.Clear();
        foreach (var unit in unitSelected)
        {
            SelectUnit(unit, false);
        }
        groundMarker.SetActive(false);
        unitSelected.Clear();
        _hasOffensiveUnits = false;
        onSelectionChanged?.Invoke();
    }

    internal void DragSelect(GameObject unit)
    {
        if (!unitSelected.Contains(unit))
        {
            unitSelected.Add(unit);
            SelectUnit(unit, true);
            AddUnitToAttackList(unit);
            onSelectionChanged?.Invoke();
        }
    }

    private void SelectUnit(GameObject unit, bool isSelected)
    {
        if (unit == null) return;
        EnableUnitMovement(unit, isSelected);
        TriggerSelectionIndicator(unit, isSelected);
    }

    private void EnableUnitMovement(GameObject unit, bool shouldMove)
    {
        if (unit == null) return;
        UnitMovement unitMovement = unit.GetComponent<UnitMovement>();
        if (unitMovement != null)
        {
            unitMovement.enabled = shouldMove;
        }
    }

    private void TriggerSelectionIndicator(GameObject unit, bool isVisible)
    {
        if (unit == null) return;
        Transform circleIndicator = unit.transform.Find("CircleIndicator");
        if (circleIndicator != null)
        {
            circleIndicator.gameObject.SetActive(isVisible);
        }
    }
}