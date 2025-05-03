using UnityEngine;

[System.Serializable]
public class UpgradeData
{
    public string upgradeName;
    public float productionTime;
    public int baseGoldCost;
    public int baseSilverCost;
    public float costMultiplier = 1.5f;
}