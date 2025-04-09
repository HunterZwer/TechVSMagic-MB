using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BuildingUI : MonoBehaviour
{
    [SerializeField] private GameObject buildingUI;
    [SerializeField] private Button[] unitButtons; // Оригинальные кнопки выбора юнитов
    [SerializeField] private Slider progressBar;
    [SerializeField] private Text progressText;
    [SerializeField] private Text queueText;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button clearQueueButton;

    [SerializeField] private Transform queueGrid; // Контейнер для отображения очереди
    [SerializeField] private GameObject queueButtonPrefab; // Префаб кнопки в очереди


    public BuildingPlacement buildingPlacement; // Ссылка на скрипт размещения
    public GameObject[] buildings;

    private Building currentBuilding;
    private List<GameObject> queueButtonsList = new List<GameObject>(); // Список кнопок в очереди

   

    private void Start()
    {
        buildingUI.SetActive(false);

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(() => currentBuilding?.CancelProduction());
        }

        if (clearQueueButton != null)
        {
            clearQueueButton.onClick.AddListener(() =>
            {
                currentBuilding?.ClearQueue();
                ClearQueueVisuals();
            });
        }
    }



    public void OnSelectBuilding(int buildingIndex)
    {
        buildingPlacement.StartBuilding(buildings[buildingIndex]);
    }



    public void ShowForBuilding(Building building)
    {
        currentBuilding = building;
        buildingUI.SetActive(true);

        for (int i = 0; i < unitButtons.Length; i++)
        {
            int unitIndex = i;
            unitButtons[i].onClick.RemoveAllListeners();
            unitButtons[i].onClick.AddListener(() =>
            {
                if (currentBuilding.StartProducingUnit(unitIndex)) 
                {
                    AddToQueue(unitIndex);
                }
            });
        }

        progressBar.value = 0;
        progressText.text = "Select Unit";
        UpdateQueueStatus(currentBuilding.QueueCount, 5);
    }

    public void UpdateProgress(float progress, string unitName)
    {
        progressBar.value = progress;
        progressText.text = $"Creating {unitName}: {(progress * 100):0}%";
    }




    public void UpdateQueueStatus(int currentCount, int maxCount)
    {
        if (queueText != null)
        {
            queueText.text = $"Queue: {currentCount}/{maxCount}";
            queueText.color = (currentCount >= maxCount) ? Color.red :
                              (currentCount > 0) ? Color.yellow : Color.white;
        }

        foreach (Button button in unitButtons)
        {
            button.interactable = currentCount < maxCount;
        }
    }

    private void AddToQueue(int unitIndex)
    {
        if (currentBuilding == null) return;

        GameObject queueButton = Instantiate(queueButtonPrefab, queueGrid);

        Text queueButtonText = queueButton.GetComponentInChildren<Text>();
        if (queueButtonText != null)
        {
            queueButtonText.text = currentBuilding.GetUnitName(unitIndex);
        }
       

        
        Image originalIcon = unitButtons[unitIndex].GetComponentInChildren<Image>();
        Image queueIcon = queueButton.GetComponentInChildren<Image>();
        if (originalIcon != null && queueIcon != null)
        {
            queueIcon.sprite = originalIcon.sprite;
        }
        
        queueButtonsList.Add(queueButton);

        
        queueButton.GetComponent<Button>().onClick.AddListener(() => RemoveFromQueue(queueButton, unitIndex));
        
    }

    private void RemoveFromQueue(GameObject queueButton, int unitIndex)
    {
        if (queueButtonsList.Contains(queueButton))
        {
            queueButtonsList.Remove(queueButton);
            Destroy(queueButton);
            currentBuilding.RemoveUnitFromQueue(unitIndex);
            UpdateQueueStatus(currentBuilding.QueueCount, 5);
            
        }

        if (queueButtonsList.Count == 0)
            currentBuilding.ClearQueue();


    }

    public void RemoveCompletedUnitFromQueue(int unitIndex)
    {
        if (queueButtonsList.Count > 0)
        {
            GameObject queueButton = queueButtonsList[0]; 
            queueButtonsList.RemoveAt(0);
            Destroy(queueButton);
        }
    }

    public void ClearQueueVisuals()
    {
        foreach (GameObject button in queueButtonsList)
        {
            Destroy(button);
        }
        queueButtonsList.Clear();
    }

    public void Hide()
    {
        //ClearQueueVisuals();
        buildingUI.SetActive(false);
        currentBuilding = null;
    }
}
