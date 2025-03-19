using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BuildingUI : MonoBehaviour
{
    [SerializeField] private GameObject buildingUI;
    [SerializeField] private Button[] unitButtons; // 4 buttons for each unit type
    [SerializeField] private Slider progressBar;
    [SerializeField] private Text progressText;
    [SerializeField] private Text queueText; // Text to show queue status
    [SerializeField] private Button cancelButton; // Button to cancel current production
    [SerializeField] private Button clearQueueButton; // Button to clear the entire queue
    
    private Building currentBuilding;
    
    private void Start()
    {
        buildingUI.SetActive(false);
        
        // Set up cancel button
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(() => {
                if (currentBuilding != null)
                {
                    currentBuilding.CancelProduction();
                }
            });
        }
        
        // Set up clear queue button
        if (clearQueueButton != null)
        {
            clearQueueButton.onClick.AddListener(() => {
                if (currentBuilding != null)
                {
                    currentBuilding.ClearQueue();
                }
            });
        }
    }
    
    public void ShowForBuilding(Building building)
    {
        currentBuilding = building;
        buildingUI.SetActive(true);
        
        // Set up button listeners
        for (int i = 0; i < unitButtons.Length; i++)
        {
            int unitIndex = i; // Important to avoid closure issues
            unitButtons[i].onClick.RemoveAllListeners();
            unitButtons[i].onClick.AddListener(() => {
                Debug.Log($"Button {unitIndex} clicked: {building.GetUnitName(unitIndex)}");
                currentBuilding.StartProducingUnit(unitIndex);
            });
        }
        
        // Reset progress bar
        progressBar.value = 0;
        progressText.text = "Select Unit";
        
        // Update queue status
        UpdateQueueStatus(currentBuilding.QueueCount, 5); // Assuming max queue size is 5
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
            
            // Option: Color-code the text based on queue fullness
            if (currentCount >= maxCount)
            {
                queueText.color = Color.red; // Queue full
            }
            else if (currentCount > 0)
            {
                queueText.color = Color.yellow; // Units in queue
            }
            else
            {
                queueText.color = Color.white; // Empty queue
            }
        }
        
        // Disable unit buttons if queue is full
        if (currentCount >= maxCount)
        {
            foreach (Button button in unitButtons)
            {
                button.interactable = false;
            }
        }
        else
        {
            foreach (Button button in unitButtons)
            {
                button.interactable = true;
            }
        }
    }
    
    public void Hide()
    {
        buildingUI.SetActive(false);
        currentBuilding = null;
    }
    
    // Add this to your BuildingUI class
    [SerializeField] private Transform queueIconsContainer;
    [SerializeField] private GameObject queueIconPrefab;
    private List<GameObject> queueIcons = new List<GameObject>();

    public void UpdateQueueVisuals()
    {
        // Clear existing icons
        foreach (GameObject icon in queueIcons)
        {
            Destroy(icon);
        }
        queueIcons.Clear();
    
        if (currentBuilding == null) return;
    
        // Get queue info from building
        // This would require modifying the Building class to expose queue information
        int[] queuedUnitIndices = currentBuilding.GetQueuedUnitIndices();
    
        // Create icons for each queued unit
        foreach (int unitIndex in queuedUnitIndices)
        {
            GameObject icon = Instantiate(queueIconPrefab, queueIconsContainer);
            // Set icon image or text based on unit type
            Text iconText = icon.GetComponentInChildren<Text>();
            if (iconText != null)
            {
                iconText.text = currentBuilding.GetUnitName(unitIndex);
            }
            queueIcons.Add(icon);
        }
    }
}