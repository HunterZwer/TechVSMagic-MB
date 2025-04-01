using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionMethods: MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text selectionButtonText;
    [SerializeField] protected Button _selectAllUnitsButton;
    
    public List<GameObject> unitSelected = new List<GameObject>();
    public event UnitSelectionManager.SelectionChanged onSelectionChanged;
    
    protected List<SelectedUnitData> selectedUnitsWithAttack = new List<SelectedUnitData>();
    protected static HashSet<GameObject> allPlayerUnits = new HashSet<GameObject>();
    protected bool _hasOffensiveUnits;
    
    private HashSet<GameObject> selectedUnitsSet = new HashSet<GameObject>();
    
    
    protected void SelectByClicking(GameObject unit)
    {
        DeselectAll();
        AddToSelection(unit);
        onSelectionChanged?.Invoke();
    }

    protected void MultiSelect(GameObject unit)
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

    protected void AddToSelection(GameObject unit)
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

    protected void RemoveFromSelection(GameObject unit)
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

    protected struct SelectedUnitData
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
    
    public void SelectAllPlayerUnits()
    {
        DeselectAll();
        foreach (GameObject unit in allPlayerUnits)
        {
            AddToSelection(unit);
        }
        onSelectionChanged?.Invoke();
        UpdateSelectionButtonText();
    }

    public void UpdateSelectionButtonText()
    {
        if (selectionButtonText != null)
        {
            selectionButtonText.text = $"{allPlayerUnits.Count}";
        }
    }
    
    protected void SelectAllUnitsOfSameType(GameObject unit)
    {
        if (!unit.TryGetComponent(out Unit unitComponent)) return;

        DeselectAll(); // Deselect everything first
        Unit.UnitClass selectedClass = unitComponent.unitClass;

        foreach (GameObject playerUnit in allPlayerUnits)
        {
            if (playerUnit.TryGetComponent(out Unit playerUnitComponent) &&
                playerUnitComponent.unitClass == selectedClass)
            {
                AddToSelection(playerUnit);
            }
        }

        onSelectionChanged?.Invoke();
    }
}
