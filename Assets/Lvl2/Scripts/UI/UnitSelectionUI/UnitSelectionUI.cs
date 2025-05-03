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
    private List<UnitLVL2> cachedSelectedUnits = new List<UnitLVL2>();

    private int currentPage = 0;
    private const int UNITS_PER_PAGE = 27;

    private readonly Color greenColor = Color.green;
    private readonly Color redColor = Color.red;

    private float healthUpdateTimer = 0f;
    private const float healthUpdateInterval = 0f;

    [SerializeField] private BuildingUI _buildingUIPanel;
    [SerializeField] private UpgradeBuildingUI upgradeUI;

    private void Start()
    {
        UnitSelectionManager.Instance.onSelectionChanged += UpdateSelectionUI;
        UnitLVL2.onUnitDied += HandleUnitDeath;


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
        UnitLVL2.onUnitDied -= HandleUnitDeath;
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
        selectedUnits.RemoveAll(unit => unit == null || unit.GetComponent<UnitLVL2>()?.IsDead == true);

        cachedSelectedUnits.Clear();
        foreach (GameObject unit in selectedUnits)
            cachedSelectedUnits.Add(unit.GetComponent<UnitLVL2>());

        currentPage = 0;

        if (selectedUnits.Count == 0)
        {
            HideUI();
            return;
        }

        if (selectedUnits.Count == 1)
        {
            _buildingUIPanel.Hide();
            upgradeUI.HideUI();
            ShowSingleUnitPanel(selectedUnits[0]);
        }
        else
        {
            _buildingUIPanel.Hide();
            upgradeUI.HideUI();
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

        UnitLVL2 unitLvl2Component = unit.GetComponent<UnitLVL2>();
        if (unitLvl2Component != null)
        {
            profileImage.sprite = unitLvl2Component.GetUnitIcon() ?? defaultProfileSprite;
            unitNameText.text = unitLvl2Component.InGameName;
            profileImage.gameObject.SetActive(true);
            healthSlider.gameObject.SetActive(true);
            healthText.gameObject.SetActive(true);

            string attackInfo = "Нет атаки";
            if (unitLvl2Component.TryGetComponent(out MeleeAttackController melee))
                attackInfo = $"Урон: {melee.unitDamage}";
            else if (unitLvl2Component.TryGetComponent(out RangeAttackController ranged))
                attackInfo = $"Урон: {ranged.unitDamage}";

            unitAttackText.text = attackInfo;

            unitSpeedText.text = unitLvl2Component.TryGetComponent(out NavMeshAgent agent)
                ? $"Скорость: {agent.speed}"
                : "Скорость: N/A";

            UpdateHealthUI(unitLvl2Component);
        }
    }

  
    private void ShowMultiUnitGrid(List<GameObject> selectedUnits)
    {
        panelSingleUnit.SetActive(false);
        gridMultiUnits.SetActive(true);

        UpdateGrid();
    }

    private void UpdateHealthUI(UnitLVL2 unitLvl2)
    {
        if (healthSlider != null && healthText != null && healthFill != null)
        {
            float health = unitLvl2.GetCurrentHealth();
            float maxHealth = unitLvl2.unitMaxHealth;
            float healthPercentage = health / maxHealth;

            healthSlider.maxValue = maxHealth;
            healthSlider.value = health; 

            healthText.text = $"{Mathf.Round(health)} / {maxHealth}";
            healthFill.color = Color.Lerp(redColor, greenColor, healthPercentage);
        }
    }

    private void UpdateGrid()
    {
        int startIndex = currentPage * UNITS_PER_PAGE;
        int endIndex = Mathf.Min(startIndex + UNITS_PER_PAGE, cachedSelectedUnits.Count);

        
        UpdatePageButtons();

        

        
        while (iconInstances.Count < UNITS_PER_PAGE)
        {
            GameObject newIcon = Instantiate(unitIconPrefab, unitIconsContainer);
            iconInstances.Add(newIcon);
        }

       
        for (int i = 0; i < iconInstances.Count; i++)
        {
            GameObject icon = iconInstances[i];

            if (i + startIndex < cachedSelectedUnits.Count)
            {
                UnitLVL2 unitLvl2Component = cachedSelectedUnits[i + startIndex];

                icon.GetComponent<Image>().sprite = unitLvl2Component.GetUnitIcon();
                icon.SetActive(true);

                Slider healthSlider = icon.GetComponentInChildren<Slider>();
                if (healthSlider != null)
                {
                    healthSlider.maxValue = unitLvl2Component.unitMaxHealth;
                    healthSlider.value = unitLvl2Component.GetCurrentHealth();
                }

                
                Button button = icon.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => FocusOnUnit(unitLvl2Component));
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
    private void FocusOnUnit(UnitLVL2 unitLvl2)
    {
        if (mainCamera != null && unitLvl2 != null)
        {
            float baseSize = 5f; 
            float baseDistance = 10f; 

            
            float sizeFactor = mainCamera.orthographicSize / baseSize;
            float adjustedDistance = baseDistance * sizeFactor;

            Vector3 unitPosition = unitLvl2.transform.position;

      
            Vector3 offset = new Vector3(-adjustedDistance, adjustedDistance * 1.3f, -adjustedDistance);
            mainCamera.transform.position = unitPosition + offset;

          
            mainCamera.transform.rotation = Quaternion.Euler(45f, 45f, 0f);
        }
    }





    private void RefreshGridHealth()
    {
        int startIndex = currentPage * UNITS_PER_PAGE;
        for (int i = 0; i < iconInstances.Count && (i + startIndex) < cachedSelectedUnits.Count; i++)
        {
            UnitLVL2 unitLvl2Component = cachedSelectedUnits[i + startIndex];
            if (unitLvl2Component == null) continue;

            Slider healthSlider = iconInstances[i].GetComponentInChildren<Slider>();
            if (healthSlider != null)
            {
                healthSlider.maxValue = unitLvl2Component.unitMaxHealth;
                healthSlider.value = unitLvl2Component.GetCurrentHealth(); // Убираем анимацию, теперь обновляется сразу
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

    private void HandleUnitDeath(UnitLVL2 deadUnitLvl2)
    {
        UpdateSelectionUI();
    }
}
