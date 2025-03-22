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
    private List<Unit> cachedSelectedUnits = new List<Unit>();

    private readonly Color greenColor = Color.green;
    private readonly Color redColor = Color.red;

    private float healthUpdateTimer = 0f;
    private const float healthUpdateInterval = 0.2f;

    private void Start()
    {
        UnitSelectionManager.Instance.onSelectionChanged += UpdateSelectionUI;
        Unit.onUnitDied += HandleUnitDeath;
        UpdateSelectionUI();
    }

    private void OnDestroy()
    {
        UnitSelectionManager.Instance.onSelectionChanged -= UpdateSelectionUI;
        Unit.onUnitDied -= HandleUnitDeath;
    }

    private void Update()
    {
        healthUpdateTimer += Time.deltaTime;
        if (healthUpdateTimer >= healthUpdateInterval)
        {
            healthUpdateTimer = 0f;
            RefreshGridHealth();
        }
    }

    private void UpdateSelectionUI()
    {
        List<GameObject> selectedUnits = UnitSelectionManager.Instance.unitSelected;
        selectedUnits.RemoveAll(unit => !unit.TryGetComponent<Unit>(out var u) || u.IsDead);

        cachedSelectedUnits.Clear();
        foreach (GameObject unit in selectedUnits)
            if (unit.TryGetComponent<Unit>(out var u))
                cachedSelectedUnits.Add(u);

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
        healthSlider.gameObject.SetActive(false);
        healthText.gameObject.SetActive(false);
        ClearIcons();
    }

    private void ShowSingleUnitPanel(GameObject unit)
    {
        panelSingleUnit.SetActive(true);
        gridMultiUnits.SetActive(false);

        unit.TryGetComponent(out Unit unitComponent);
        if (unitComponent)
        {
            profileImage.sprite = unitComponent.GetUnitIcon() ?? defaultProfileSprite;
            unitNameText.text = unit.name;
            profileImage.gameObject.SetActive(true);
            healthSlider.gameObject.SetActive(true);
            healthText.gameObject.SetActive(true);

            string attackInfo = "Нет атаки";
            if (unitComponent.TryGetComponent(out MeleeAttackController melee))
                attackInfo = $"Урон: {melee.unitDamage}";
            else if (unitComponent.TryGetComponent(out RangeAttackController ranged))
                attackInfo = $"Урон: {ranged.unitDamage}";

            unitAttackText.text = attackInfo;

            unitSpeedText.text = unitComponent.TryGetComponent(out NavMeshAgent agent)
                ? $"Скорость: {agent.speed}"
                : "Скорость: N/A";

            UpdateHealthUI(unitComponent);
        }
    }

    private void ShowMultiUnitGrid(List<GameObject> selectedUnits)
    {
        panelSingleUnit.SetActive(false);
        gridMultiUnits.SetActive(true);

        selectedUnits[0].TryGetComponent(out Unit firstUnit);
        if (firstUnit)
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
        if (healthSlider && healthText && healthFill)
        {
            float health = unit.GetCurrentHealth();
            float maxHealth = unit.unitMaxHealth;
            float healthPercentage = health / maxHealth;

            healthSlider.maxValue = maxHealth;

           
            healthSlider.value = Mathf.Lerp(healthSlider.value, health, Time.deltaTime * 10f);

            healthText.text = $"{Mathf.Round(health)} / {maxHealth}";

            healthFill.color = Color.Lerp(redColor, greenColor, healthPercentage);
        }
    }

    private void UpdateGrid(List<GameObject> selectedUnits)
    {
        int existingIcons = iconInstances.Count;
        int newIconsNeeded = selectedUnits.Count - existingIcons;

        for (int i = 0; i < newIconsNeeded; i++)
        {
            GameObject newIcon = Instantiate(unitIconPrefab, unitIconsContainer);
            iconInstances.Add(newIcon);
        }

        for (int i = 0; i < selectedUnits.Count; i++)
        {
            selectedUnits[i].TryGetComponent(out Unit unitComponent);
            if (!unitComponent) continue;

            GameObject icon = iconInstances[i];
            icon.TryGetComponent(out Image iconImage);
            iconImage.sprite = unitComponent.GetUnitIcon();

            Slider healthSlider = icon.GetComponentInChildren<Slider>();
            if (healthSlider)
            {
                healthSlider.maxValue = unitComponent.unitMaxHealth;
                healthSlider.value = unitComponent.GetCurrentHealth();
            }
            icon.SetActive(true);
        }

        for (int i = selectedUnits.Count; i < iconInstances.Count; i++)
        {
            iconInstances[i].SetActive(false);
        }
    }

    private void RefreshGridHealth()
    {
        for (int i = 0; i < cachedSelectedUnits.Count && i < iconInstances.Count; i++)
        {
            Unit unitComponent = cachedSelectedUnits[i];
            if (!unitComponent) continue;

            Slider healthSlider = iconInstances[i].GetComponentInChildren<Slider>();
            if (healthSlider)
            {
                healthSlider.maxValue = unitComponent.unitMaxHealth;

                
                healthSlider.value = Mathf.Lerp(healthSlider.value, unitComponent.GetCurrentHealth(), Time.deltaTime * 10f);
            }
        }

        if (cachedSelectedUnits.Count > 0)
        {
            UpdateHealthUI(cachedSelectedUnits[0]); 
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
