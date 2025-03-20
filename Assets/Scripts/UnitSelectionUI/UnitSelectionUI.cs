using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.AI;

public class UnitSelectionUI : MonoBehaviour
{
    public GameObject panelSingleUnit;
    public GameObject gridMultiUnits;
    public TextMeshProUGUI unitNameText;
    public TextMeshProUGUI unitAttackText;
    public TextMeshProUGUI unitSpeedText;
    public GameObject unitIconPrefab;
    public Transform unitIconsContainer;

    public Image profileImage;
    public Sprite defaultProfileSprite;

    public Slider healthSlider;
    public TextMeshProUGUI healthText;
    public Image healthFill;

    private List<GameObject> iconInstances = new List<GameObject>();

    private readonly Color greenColor = Color.green;
    private readonly Color yellowColor = Color.yellow;
    private readonly Color redColor = Color.red;

    private void Start()
    {
        UnitSelectionManager.Instance.onSelectionChanged += UpdateSelectionUI;
        Unit.onUnitStatsChanged += UpdateSelectionUI;
        Unit.onUnitClassChanged += UpdateSelectionUI;
        Unit.onUnitDied += HandleUnitDeath;
        UpdateSelectionUI();
    }

    private void OnDestroy()
    {
        UnitSelectionManager.Instance.onSelectionChanged -= UpdateSelectionUI;
        Unit.onUnitStatsChanged -= UpdateSelectionUI;
        Unit.onUnitClassChanged -= UpdateSelectionUI;
        Unit.onUnitDied -= HandleUnitDeath;
    }

    private void UpdateSelectionUI()
    {
        List<GameObject> selectedUnits = UnitSelectionManager.Instance.unitSelected;

        // 🔥 Удаляем мертвых юнитов из списка
        selectedUnits.RemoveAll(unit => unit == null || unit.GetComponent<Unit>()?.IsDead == true);

        UpdateGridHealth();

        if (selectedUnits.Count == 0)
        {
            HideUI();
            return;
        }

        if (selectedUnits.Count == 1)
        {
            ShowSingleUnitPanel(selectedUnits[0]);
        }
        else
        {
            ShowMultiUnitGrid(selectedUnits);
        }
    }

    private void HideUI()
    {
        panelSingleUnit.SetActive(false);
        gridMultiUnits.SetActive(false);
        profileImage.sprite = defaultProfileSprite;
        profileImage.gameObject.SetActive(false);
        healthSlider.gameObject.SetActive(false);
        healthText.gameObject.SetActive(false);
        ClearIcons();
    }

    private void ShowSingleUnitPanel(GameObject unit)
    {
        panelSingleUnit.SetActive(true);
        gridMultiUnits.SetActive(false);

        Unit unitComponent = unit.GetComponent<Unit>();
        if (unitComponent != null)
        {
            profileImage.sprite = unitComponent.GetUnitIcon() ?? defaultProfileSprite;
            unitNameText.text = unit.name;

            profileImage.gameObject.SetActive(true);
            healthSlider.gameObject.SetActive(true);
            healthText.gameObject.SetActive(true);

            string attackInfo = "Нет атаки";
            MeleeAttackController melee = unit.GetComponent<MeleeAttackController>();
            RangeAttackController ranged = unit.GetComponent<RangeAttackController>();

            if (melee != null)
                attackInfo = $"Урон: {melee.unitDamage}";
            else if (ranged != null)
                attackInfo = $"Урон: {ranged.projectileDamage}";

            unitAttackText.text = attackInfo;

            NavMeshAgent agent = unit.GetComponent<NavMeshAgent>();
            unitSpeedText.text = agent != null ? $"Скорость: {agent.speed}" : "Скорость: N/A";

            UpdateHealthUI(unitComponent);
        }
    }

    private void ShowMultiUnitGrid(List<GameObject> selectedUnits)
    {
        panelSingleUnit.SetActive(false);
        gridMultiUnits.SetActive(true);

        // 🔥 Оставляем профиль и слайдер здоровья для первого юнита
        Unit firstUnit = selectedUnits[0].GetComponent<Unit>();
        if (firstUnit != null)
        {
            profileImage.sprite = firstUnit.GetUnitIcon() ?? defaultProfileSprite;
            profileImage.gameObject.SetActive(true);
            healthSlider.gameObject.SetActive(true);
            healthText.gameObject.SetActive(true);
            UpdateHealthUI(firstUnit);
        }
        else
        {
            profileImage.gameObject.SetActive(false);
            healthSlider.gameObject.SetActive(false);
            healthText.gameObject.SetActive(false);
        }

        UpdateGrid(selectedUnits);
    }

    private void UpdateHealthUI(Unit unit)
    {
        if (healthSlider != null && healthText != null && healthFill != null)
        {
            float health = unit.GetCurrentHealth();
            float maxHealth = unit.unitMaxHealth;
            float healthPercentage = health / maxHealth;

            healthSlider.maxValue = maxHealth;
            healthSlider.value = health;
            healthText.text = $"{health} / {maxHealth}";

            healthFill.color = Color.Lerp(redColor, greenColor, healthPercentage);
        }
    }

    private void UpdateGrid(List<GameObject> selectedUnits)
    {
        ClearIcons();

        foreach (GameObject unit in selectedUnits)
        {
            Unit unitComponent = unit.GetComponent<Unit>();
            if (unitComponent == null || unitComponent.GetUnitIcon() == null) continue;

            GameObject newIcon = Instantiate(unitIconPrefab, unitIconsContainer);
            newIcon.GetComponent<Image>().sprite = unitComponent.GetUnitIcon();

            // Найдём слайдер внутри нового объекта
            Slider healthSlider = newIcon.GetComponentInChildren<Slider>();
            if (healthSlider != null)
            {
                healthSlider.maxValue = unitComponent.unitMaxHealth;
                healthSlider.value = unitComponent.GetCurrentHealth();
            }

            // Добавляем в список для динамического обновления
            iconInstances.Add(newIcon);
        }
    }


    private void UpdateGridHealth()
    {
        for (int i = 0; i < iconInstances.Count; i++)
        {
            if (i >= UnitSelectionManager.Instance.unitSelected.Count) break;

            GameObject unit = UnitSelectionManager.Instance.unitSelected[i];
            Unit unitComponent = unit.GetComponent<Unit>();
            if (unitComponent == null) continue;

            Slider healthSlider = iconInstances[i].GetComponentInChildren<Slider>();
            if (healthSlider == null) continue;

            float health = unitComponent.GetCurrentHealth();
            float maxHealth = unitComponent.unitMaxHealth;

            // Обновляем значение слайдера
            healthSlider.maxValue = maxHealth;
            healthSlider.value = health;
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

    private void HandleUnitDeath(Unit deadUnit)
    {
        UpdateSelectionUI();
    }
}
