using UnityEngine;
using System.IO;

public static class JsonLoader
{
    // Load unitLvl2 stats based on the unitLvl2's class and team
    public static UnitStats LoadUnitStats(UnitLVL2.UnitClass unitClass, bool isPlayer)
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

    // Map the UnitClass enum to the corresponding unitLvl2 type string
    private static string GetUnitTypeFromClass(UnitLVL2.UnitClass unitClass)
    {
        switch (unitClass)
        {
            case UnitLVL2.UnitClass.Knight:
                return "Knight";
            case UnitLVL2.UnitClass.Archer:
                return "Archer";
            case UnitLVL2.UnitClass.Shaman:
                return "Shaman";
            case UnitLVL2.UnitClass.Brute:
                return "Brute";
            default:
                Debug.LogWarning($"Unknown unitLvl2 class: {unitClass}, defaulting to Brute");
                return "Brute";
        }
    }
}
