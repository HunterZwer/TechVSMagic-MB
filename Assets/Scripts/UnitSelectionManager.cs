using UnityEngine;
using System.Collections.Generic;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; private set; }

    public List<GameObject> allUnitSelected = new List<GameObject>();
    public List<GameObject> unitSelected = new List<GameObject>();

    public LayerMask clickable; // Fixed typo in variable name
    public LayerMask ground;
    public LayerMask attackable;
    public GameObject groundMarker;
    public bool attackCursorVisible;

    private Camera cam;

    public delegate void SelectionChanged();
    public event SelectionChanged onSelectionChanged; // Event for updating UI

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
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, clickable))
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
            else
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    DeselectAll();
                }
            }
        }

        if (Input.GetMouseButtonDown(1) && unitSelected.Count > 0)
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, clickable))
            {
                groundMarker.transform.position = hit.point;
                groundMarker.SetActive(true);
            }
        }

        // Attack logic
        if (unitSelected.Count > 0 && AtLeastOneOffensiveUnit(unitSelected))
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, attackable))
            {
                attackCursorVisible = true;

                if (Input.GetMouseButtonDown(1))
                {
                    Transform target = hit.transform;
                    foreach (GameObject unit in unitSelected)
                    {
                        if (unit != null && unit.GetComponent<AttackController>())
                        {
                            unit.GetComponent<AttackController>().targetToAttack = target;
                        }
                    }
                }
            }
            else
            {
                attackCursorVisible = false;
            }
        }
    }

    private bool AtLeastOneOffensiveUnit(List<GameObject> unitSelected)
    {
        foreach (GameObject unit in unitSelected)
        {
            if (unit != null && unit.GetComponent<AttackController>())
            {
                return true;
            }
        }
        return false;
    }

    private void SelectByClicking(GameObject unit)
    {
        DeselectAll();
        unitSelected.Add(unit);
        SelectUnit(unit, true);
        onSelectionChanged?.Invoke(); // Update UI
    }

    private void MultiSelect(GameObject unit)
    {
        if (!unitSelected.Contains(unit))
        {
            unitSelected.Add(unit);
            SelectUnit(unit, true);
        }
        else
        {
            SelectUnit(unit, false);
            unitSelected.Remove(unit);
        }
        onSelectionChanged?.Invoke(); // Update UI
    }

    public void DeselectAll()
    {
        // Remove null references from the list
        unitSelected.RemoveAll(unit => unit == null);

        foreach (var unit in unitSelected)
        {
            SelectUnit(unit, false);
        }
        groundMarker.SetActive(false);
        unitSelected.Clear();
        onSelectionChanged?.Invoke(); // Update UI
    }

    internal void DragSelect(GameObject unit)
    {
        if (!unitSelected.Contains(unit))
        {
            unitSelected.Add(unit);
            SelectUnit(unit, true);
        }
        onSelectionChanged?.Invoke(); // Update UI
    }

    private void SelectUnit(GameObject unit, bool isSelected)
    {
        if (unit == null) return; // Skip if the unit is destroyed

        EnableUnitMovement(unit, isSelected);
        TriggerSelectionIndicator(unit, isSelected);
    }

    private void EnableUnitMovement(GameObject unit, bool shouldMove)
    {
        if (unit == null) return; // Skip if the unit is destroyed

        UnitMovement unitMovement = unit.GetComponent<UnitMovement>();
        if (unitMovement != null)
        {
            unitMovement.enabled = shouldMove;
        }
    }

    private void TriggerSelectionIndicator(GameObject unit, bool isVisible)
    {
        if (unit == null) return; // Skip if the unit is destroyed

        Transform circleIndicator = unit.transform.Find("CircleIndicator");
        if (circleIndicator != null)
        {
            circleIndicator.gameObject.SetActive(isVisible);
        }
    }
}