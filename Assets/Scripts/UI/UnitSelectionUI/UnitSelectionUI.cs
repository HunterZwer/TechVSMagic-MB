using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Collections;

public class UnitSelectionUI : MonoBehaviour
{

    public Camera mainCamera;
    public GameObject panelSingleUnit;
    public GameObject gridMultiUnits;
    public TextMeshProUGUI unitNameText;
    public TextMeshProUGUI unitAttackText;
    public TextMeshProUGUI unitSpeedText;
    public GameObject unitIconPrefab;
    public Transform unitIconsContainer;

   
    public Button[] pageButtons; 

    public Image profileImage;
    public Sprite defaultProfileSprite;

    public Slider healthSlider;
    public TextMeshProUGUI healthText;
    public Image healthFill;

    private List<GameObject> iconInstances = new List<GameObject>();
    private List<Unit> cachedSelectedUnits = new List<Unit>();

    private int currentPage = 0;
    private const int UNITS_PER_PAGE = 27;

    private readonly Color greenColor = Color.green;
    private readonly Color redColor = Color.red;

    private float healthUpdateTimer = 0f;
    private const float healthUpdateInterval = 0.2f;

    [SerializeField] private BuildingUI _buildingUIPanel;

    private void Start()
    {
        UnitSelectionManager.Instance.onSelectionChanged += UpdateSelectionUI;
        Unit.onUnitDied += HandleUnitDeath;


        for (int i = 0; i < pageButtons.Length; i++)
        {
            int pageIndex = i;
            pageButtons[i].onClick.AddListener(() => SelectPage(pageIndex));
        }

        UpdateSelectionUI();
    }

    private void SelectPage(int pageIndex)
    {
        if (pageIndex * UNITS_PER_PAGE < cachedSelectedUnits.Count)
        {
            currentPage = pageIndex;
            UpdateGrid();
        }
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

        foreach (Button btn in pageButtons)
        {
            btn.gameObject.SetActive(false);
        }
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

        // Обновляем кнопки страниц
        UpdatePageButtons();

        

        // Создаём недостающие иконки
        while (iconInstances.Count < UNITS_PER_PAGE)
        {
            GameObject newIcon = Instantiate(unitIconPrefab, unitIconsContainer);
            iconInstances.Add(newIcon);
        }

        // Обновляем существующие иконки
        for (int i = 0; i < iconInstances.Count; i++)
        {
            GameObject icon = iconInstances[i];

            if (i + startIndex < cachedSelectedUnits.Count)
            {
                Unit unitComponent = cachedSelectedUnits[i + startIndex];

                icon.GetComponent<Image>().sprite = unitComponent.GetUnitIcon();
                icon.SetActive(true);

                Slider healthSlider = icon.GetComponentInChildren<Slider>();
                if (healthSlider != null)
                {
                    healthSlider.maxValue = unitComponent.unitMaxHealth;
                    healthSlider.value = unitComponent.GetCurrentHealth();
                }

                // Добавляем обработчик нажатия для фокусировки на юните
                Button button = icon.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => FocusOnUnit(unitComponent));
                }
            }
            else
            {
                icon.SetActive(false);
            }
        }
    }

    private void UpdatePageButtons()
    {
        int totalPages = Mathf.CeilToInt((float)cachedSelectedUnits.Count / UNITS_PER_PAGE);

        for (int i = 0; i < pageButtons.Length; i++)
        {
            if (i < totalPages)
            {
                pageButtons[i].gameObject.SetActive(true);
                pageButtons[i].interactable = i != currentPage;
            }
            else
            {
                pageButtons[i].gameObject.SetActive(false);
            }
        }
    }
    private void FocusOnUnit(Unit unit)
    {
        if (mainCamera != null && unit != null)
        {
            float baseSize = 5f; // Базовый размер камеры
            float baseDistance = 10f; // Базовое расстояние

            // Коэффициент изменения отступа
            float sizeFactor = mainCamera.orthographicSize / baseSize;
            float adjustedDistance = baseDistance * sizeFactor;

            Vector3 unitPosition = unit.transform.position;

            // Смещаем камеру так, чтобы она смотрела ровно на центр юнита
            Vector3 offset = new Vector3(-adjustedDistance, adjustedDistance * 1.3f, -adjustedDistance);
            mainCamera.transform.position = unitPosition + offset;

            // Фиксируем угол
            mainCamera.transform.rotation = Quaternion.Euler(45f, 45f, 0f);
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
