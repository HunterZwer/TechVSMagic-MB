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

    public Button prevPageButton;
    public Button nextPageButton;

    public Image profileImage;
    public Sprite defaultProfileSprite;

    public Slider healthSlider;
    public TextMeshProUGUI healthText;
    public Image healthFill;

    private List<GameObject> iconInstances = new List<GameObject>();
    private List<Unit> cachedSelectedUnits = new List<Unit>();

    private int currentPage = 0;
    private const int UNITS_PER_PAGE = 21;

    private readonly Color greenColor = Color.green;
    private readonly Color redColor = Color.red;

    private float healthUpdateTimer = 0f;
    private const float healthUpdateInterval = 0.2f;

    [SerializeField] private BuildingUI _buildingUIPanel;

    private void Start()
    {
        UnitSelectionManager.Instance.onSelectionChanged += UpdateSelectionUI;
        Unit.onUnitDied += HandleUnitDeath;

        prevPageButton.onClick.AddListener(PrevPage);
        nextPageButton.onClick.AddListener(NextPage);

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
        selectedUnits.RemoveAll(unit => unit == null || unit.GetComponent<Unit>()?.IsDead == true);

        cachedSelectedUnits.Clear();
        foreach (GameObject unit in selectedUnits)
            cachedSelectedUnits.Add(unit.GetComponent<Unit>());

        currentPage = 0;

        if (selectedUnits.Count == 0)
        {
            HideUI();
            return;
        }

        if (selectedUnits.Count == 1)
        {
            _buildingUIPanel.Hide();
            ShowSingleUnitPanel(selectedUnits[0]);
        }
        else
        {
            _buildingUIPanel.Hide();
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

        Unit unitComponent = unit.GetComponent<Unit>();
        if (unitComponent != null)
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
                attackInfo = $"Урон: {ranged.projectileDamage}";

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

        UpdateGrid();
    }

    private void UpdateHealthUI(Unit unit)
    {
        if (healthSlider != null && healthText != null && healthFill != null)
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

    private void UpdateGrid()
    {
        int startIndex = currentPage * UNITS_PER_PAGE;
        int endIndex = Mathf.Min(startIndex + UNITS_PER_PAGE, cachedSelectedUnits.Count);

     //   prevPageButton.gameObject.SetActive(currentPage > 0);
      //  nextPageButton.gameObject.SetActive(endIndex < cachedSelectedUnits.Count);

        // Создаём недостающие иконки
        while (iconInstances.Count < UNITS_PER_PAGE)
        {
            GameObject newIcon = Instantiate(unitIconPrefab, unitIconsContainer);
            iconInstances.Add(newIcon);
        }

        // Обновляем существующие иконки
        for (int i = 0; i < iconInstances.Count; i++)
        {
            if (i + startIndex < cachedSelectedUnits.Count)
            {
                Unit unitComponent = cachedSelectedUnits[i + startIndex];
                GameObject icon = iconInstances[i];

                icon.GetComponent<Image>().sprite = unitComponent.GetUnitIcon();
                Slider healthSlider = icon.GetComponentInChildren<Slider>();
                if (healthSlider != null)
                {
                    healthSlider.maxValue = unitComponent.unitMaxHealth;
                    healthSlider.value = unitComponent.GetCurrentHealth();
                }
                icon.SetActive(true);
            }
            else
            {
                iconInstances[i].SetActive(false);
            }
        }
    }

    private void RefreshGridHealth()
    {
        int startIndex = currentPage * UNITS_PER_PAGE;
        for (int i = 0; i < iconInstances.Count && (i + startIndex) < cachedSelectedUnits.Count; i++)
        {
            Unit unitComponent = cachedSelectedUnits[i + startIndex];
            if (unitComponent == null) continue;

            Slider healthSlider = iconInstances[i].GetComponentInChildren<Slider>();
            if (healthSlider != null)
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

    private void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdateGrid();
        }
    }

    private void NextPage()
    {
        if ((currentPage + 1) * UNITS_PER_PAGE < cachedSelectedUnits.Count)
        {
            currentPage++;
            UpdateGrid();
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
