using System.Collections.Generic;
using UnityEngine;

public class UnitRegistryManager: SelectionMethods
{
    public static UnitRegistryManager Instance { get; private set; }
    public static void RegisterPlayerUnit(GameObject unit)
    {
        allPlayerUnits.Add(unit);
        UnitSelectionManager.Instance?.UpdateSelectionButtonText();
    }

    public static void UnregisterPlayerUnit(GameObject unit)
    {
        allPlayerUnits.Remove(unit);
        UnitSelectionManager.Instance?.UpdateSelectionButtonText();
    }
    public static void RegisterEnemyUnit(GameObject unit)
    {
        allEnemyUnits.Add(unit);
    }

    public static void UnregisterEnemyUnit(GameObject unit)
    {
        allEnemyUnits.Remove(unit);
    }

    public static HashSet<GameObject> ReturnAllPlayerUnits()
    {
        return allPlayerUnits;
    }    
    
    public static HashSet<GameObject> ReturnAllEnemyUnits()
    {
        return allEnemyUnits;
    }
}
