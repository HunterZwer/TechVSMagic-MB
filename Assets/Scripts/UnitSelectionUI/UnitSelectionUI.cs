using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UnitSelectionUI : MonoBehaviour
{
    public GameObject panel; // Основная панель UI
    public TextMeshProUGUI unitNamesText;  // Имена юнитов
    public TextMeshProUGUI unitAttackText; // Урон юнитов
    public GameObject unitIconPrefab;      // Префаб иконки юнита
    public Transform unitIconsContainer;   // Контейнер для иконок (Grid Layout Group)

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

        panel.SetActive(true);
        unitNamesText.text = "";
        unitAttackText.text = "";
        ClearIcons();

        foreach (GameObject unit in selectedUnits)
        {
            Unit unitComponent = unit.GetComponent<Unit>();
            if (unitComponent == null) continue;

            // Добавляем имя юнита
            unitNamesText.text += $"- {unit.name}\n";

            // Определяем урон юнита
            string attackInfo = "Нет атаки";
            MeleeAttackController melee = unit.GetComponent<MeleeAttackController>();
            RangeAttackController ranged = unit.GetComponent<RangeAttackController>();

            if (melee != null)
            {
                attackInfo = $"Урон: {melee.unitDamage}";
            }
            else if (ranged != null)
            {
                attackInfo = $"Урон: {ranged.projectileDamage}";
            }

            unitAttackText.text += attackInfo + "\n";

            // Добавляем иконку юнита
            if (unitComponent.GetUnitIcon() != null)
            {
                GameObject newIcon = Instantiate(unitIconPrefab, unitIconsContainer);
                newIcon.GetComponent<Image>().sprite = unitComponent.GetUnitIcon();
                iconInstances.Add(newIcon);
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
