using UnityEngine;
using System.IO;

public static class JsonLoader
{
    // Load unit stats based on the unit's class and team
    public static UnitStats LoadUnitStats(Unit.UnitClass unitClass, bool isPlayer)
    {
        string team = isPlayer ? "TeamA" : "TeamB";
        string unitType = GetUnitTypeFromClass(unitClass);
        string jsonPath = Path.Combine("config", "Units", team, unitType);

        return LoadJsonData<UnitStats>(jsonPath);
    }

    // Generic method to load JSON data from a specified path
    private static T LoadJsonData<T>(string path) where T : new()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(path);
        if (jsonFile != null)
        {
            return JsonUtility.FromJson<T>(jsonFile.text);
        }
        else
        {
            Debug.LogError($"Failed to load JSON data from: {path}");
            return new T(); // Return a new instance of the type if loading fails
        }
    }

    // Map the UnitClass enum to the corresponding unit type string
    private static string GetUnitTypeFromClass(Unit.UnitClass unitClass)
    {
        switch (unitClass)
        {
            case Unit.UnitClass.Knight:
                return "Knight";
            case Unit.UnitClass.Archer:
                return "Archer";
            case Unit.UnitClass.Shaman:
                return "Shaman";
            case Unit.UnitClass.Brute:
                return "Brute";
            default:
                Debug.LogWarning($"Unknown unit class: {unitClass}, defaulting to Brute");
                return "Brute";
        }
    }
}
