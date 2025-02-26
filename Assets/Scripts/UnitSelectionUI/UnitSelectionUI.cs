using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UnitSelectionUI : MonoBehaviour
{
    public GameObject panel; // Панель UI
    public TextMeshProUGUI countText;      // Количество юнитов
    public TextMeshProUGUI unitNamesText;  // Имена юнитов
    public TextMeshProUGUI unitStatsText;  // Характеристики юнитов
    public GameObject unitIconPrefab;      // Префаб иконки
    public Transform unitIconsContainer;   // Контейнер с Grid Layout Group

    private List<GameObject> iconInstances = new List<GameObject>();

    private void Start()
    {
        UnitSelectionManager.Instance.onSelectionChanged += UpdateSelectionUI;
        Unit.onUnitStatsChanged += UpdateSelectionUI;
        Unit.onUnitClassChanged += UpdateSelectionUI;
        UpdateSelectionUI();
    }

    private void OnDestroy()
    {
        UnitSelectionManager.Instance.onSelectionChanged -= UpdateSelectionUI;
        Unit.onUnitStatsChanged -= UpdateSelectionUI;
        Unit.onUnitClassChanged -= UpdateSelectionUI;
    }

    private void UpdateSelectionUI()
    {
        List<GameObject> selectedUnits = UnitSelectionManager.Instance.unitSelected;

        if (selectedUnits.Count == 0)
        {
            panel.SetActive(false);
            ClearIcons();
            return;
        }

        panel.SetActive(true);
        countText.text = $"Выбрано: {selectedUnits.Count}";

        unitNamesText.text = "";
        unitStatsText.text = "";
        ClearIcons();

        foreach (GameObject unit in selectedUnits)
        {
            Unit unitComponent = unit.GetComponent<Unit>();
            if (unitComponent != null)
            {
                unitNamesText.text += $"- {unit.name}\n";
                unitStatsText.text += $"HP: {unitComponent.GetCurrentHealth()}/{unitComponent.unitMaxHealth}\n";

                if (unitComponent.GetUnitIcon() != null)
                {
                    GameObject newIcon = Instantiate(unitIconPrefab, unitIconsContainer);
                    newIcon.GetComponent<Image>().sprite = unitComponent.GetUnitIcon();
                    iconInstances.Add(newIcon);
                }
            }
        }
    }

    private void ClearIcons()
    {
        foreach (GameObject icon in iconInstances)
        {
            Destroy(icon);
        }
        iconInstances.Clear();
    }
}
