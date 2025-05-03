using UnityEngine;

public class UpgradeBuilding : MonoBehaviour
{
    [SerializeField] private UpgradeBuildingUI upgradeUI;

    private void OnMouseDown()
    {
        BuildingUI anyBuildingUI = FindObjectOfType<BuildingUI>();
        if (anyBuildingUI != null)
            anyBuildingUI.Hide();

        upgradeUI.ShowUI();
    }

    private void OnDestroy()
    {
        if (upgradeUI != null)
            upgradeUI.HideUI();
    }

}