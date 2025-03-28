using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; private set; }

    public List<GameObject> allUnitSelected = new List<GameObject>();
    public List<GameObject> unitSelected = new List<GameObject>();
    private List<SelectedUnitData> selectedUnitsWithAttack = new List<SelectedUnitData>();
    private HashSet<GameObject> selectedUnitsSet = new HashSet<GameObject>();

    public LayerMask clickable;
    public LayerMask ground;
    public LayerMask attackable;
    [Space][SerializeField] private Renderer ClickIcon;
    public bool attackCursorVisible;

    private Camera cam;
    private bool _hasOffensiveUnits;
    private static readonly int CLCIK_TIME_PROPERTY = Shader.PropertyToID("_ClickTime");

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
            // ��������� �������� �� ���� �� UI
            if (IsPointerOverUI()) return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, clickable))
            {
                GameObject clickedUnit = hit.collider.gameObject;
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    MultiSelect(clickedUnit);
                }
                else
                {
                    SelectByClicking(clickedUnit);
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
            // ��������� �������� �� ���� �� UI
            if (IsPointerOverUI()) return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ground))
            {
                if (unitSelected.Count > 0)
                {
                    ClickIcon.transform.position = hit.point + Vector3.up * 0.01f;
                    ClickIcon.material.SetFloat(CLCIK_TIME_PROPERTY, Time.time);
                }
            }

            if (_hasOffensiveUnits && Physics.Raycast(ray, out hit, Mathf.Infinity, attackable))
            {
                Transform target = hit.transform;
                foreach (var unitData in selectedUnitsWithAttack)
                {
                    if (unitData.meleeAttack != null)
                    {
                        unitData.meleeAttack.targetToAttack = target;
                    }
                    if (unitData.rangeAttack != null)
                    {
                        unitData.rangeAttack.targetToAttack = target;
                    }
                }
            }
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
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
        AddToSelection(unit);
        onSelectionChanged?.Invoke();
    }

    private void MultiSelect(GameObject unit)
    {
        if (!unitSelected.Contains(unit))
        {
            unitSelected.Add(unit);
            SelectUnit(unit, true);
            AddToSelection(unit);
        }
        else
        {
            SelectUnit(unit, false);
            unitSelected.Remove(unit);
            RemoveFromSelection(unit);
        }
        onSelectionChanged?.Invoke();
    }

    private void AddToSelection(GameObject unit)
    {
        unitSelected.Add(unit);
        selectedUnitsSet.Add(unit);
        SelectUnit(unit, true);

        unit.TryGetComponent(out MeleeAttackController melee);
        unit.TryGetComponent(out RangeAttackController ranged);
        if (melee != null || ranged != null)
        {
            selectedUnitsWithAttack.Add(new SelectedUnitData(unit, melee, ranged));
            _hasOffensiveUnits = true;
        }
    }

    private void RemoveFromSelection(GameObject unit)
    {
        unitSelected.Remove(unit);
        selectedUnitsSet.Remove(unit);
        SelectUnit(unit, false);

        for (int i = 0; i < selectedUnitsWithAttack.Count; i++)
        {
            if (selectedUnitsWithAttack[i].unit == unit)
            {
                selectedUnitsWithAttack.RemoveAt(i);
                break;
            }
        }
        _hasOffensiveUnits = selectedUnitsWithAttack.Count > 0;
    }

    public void DeselectAll()
    {
        foreach (var unit in unitSelected)
        {
            SelectUnit(unit, false);
        }

        unitSelected.Clear();
        selectedUnitsSet.Clear();
        selectedUnitsWithAttack.Clear();
        _hasOffensiveUnits = false;
        onSelectionChanged?.Invoke();
    }

    internal void DragSelect(GameObject unit)
    {
        if (!unitSelected.Contains(unit))
        {
            unitSelected.Add(unit);
            SelectUnit(unit, true);
            onSelectionChanged?.Invoke();
        }
    }

    private void SelectUnit(GameObject unit, bool isSelected)
    {
        unit.TryGetComponent(out Unit unitComponent);
        if (unitComponent == null || unitComponent.circleIndicator == null) return;
        unitComponent.circleIndicator.gameObject.SetActive(isSelected);
    }

    private struct SelectedUnitData
    {
        public GameObject unit;
        public MeleeAttackController meleeAttack;
        public RangeAttackController rangeAttack;

        public SelectedUnitData(GameObject unit, MeleeAttackController melee, RangeAttackController ranged)
        {
            this.unit = unit;
            this.meleeAttack = melee;
            this.rangeAttack = ranged;
        }
    }
}
