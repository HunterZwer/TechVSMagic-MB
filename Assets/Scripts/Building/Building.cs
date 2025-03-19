using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Building : MonoBehaviour
{
    [System.Serializable]
    public class UnitData
    {
        public string unitName;
        public GameObject unitPrefab;
        public float productionTime;
    }
    
    [SerializeField] private UnitData[] availableUnits; // Your 4 different unit types
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private BuildingUI uiManager;
    [SerializeField] private int maxQueueSize = 5; // Maximum number of units that can be queued
    
    private bool isProducing = false;
    private Queue<int> productionQueue = new Queue<int>(); // Queue of unit indices to produce
    
    public int QueueCount => productionQueue.Count;
    public bool IsQueueFull => productionQueue.Count >= maxQueueSize;
    
    private void OnMouseDown()
    {
        // When player clicks on building, show UI
        Debug.Log($"Building clicked: {gameObject.name}");
        uiManager.ShowForBuilding(this);
    }
    
    public string GetUnitName(int index)
    {
        if (index >= 0 && index < availableUnits.Length)
            return availableUnits[index].unitName;
        return "Unknown";
    }
    
    public void StartProducingUnit(int unitIndex)
    {
        if (unitIndex < 0 || unitIndex >= availableUnits.Length)
        {
            Debug.LogError($"Invalid unit index: {unitIndex}");
            return;
        }
        
        // Check if the queue is full
        if (IsQueueFull)
        {
            Debug.LogWarning($"Production queue is full! Cannot add {availableUnits[unitIndex].unitName}");
            // You could show a message to the player here
            return;
        }
        
        // Add unit to the queue
        productionQueue.Enqueue(unitIndex);
        Debug.Log($"Added {availableUnits[unitIndex].unitName} to production queue. Queue size: {productionQueue.Count}");
        
        // Update UI to show queue status
        uiManager.UpdateQueueStatus(productionQueue.Count, maxQueueSize);
        
        // If we're not already producing, start the production process
        if (!isProducing)
        {
            ProcessProductionQueue();
        }
    }
    
    private void ProcessProductionQueue()
    {
        if (productionQueue.Count == 0)
        {
            isProducing = false;
            uiManager.UpdateProgress(0, "Idle");
            return;
        }
        
        isProducing = true;
        int unitIndex = productionQueue.Peek(); // Look at the next unit but don't remove it yet
        StartCoroutine(ProduceUnit(unitIndex));
    }
    
    private IEnumerator ProduceUnit(int unitIndex)
    {
        UnitData unit = availableUnits[unitIndex];
        float timer = 0;
        
        Debug.Log($"Starting production of {unit.unitName} - Production time: {unit.productionTime}s");
        
        while (timer < unit.productionTime)
        {
            timer += Time.deltaTime;
            float progress = timer / unit.productionTime;
            
            // Update progress bar
            uiManager.UpdateProgress(progress, unit.unitName);
            
            yield return null;
        }
        
        // Remove the unit from the queue now that it's complete
        productionQueue.Dequeue();
        
        // Spawn the unit
        GameObject newUnit = Instantiate(unit.unitPrefab, spawnPoint.position, spawnPoint.rotation);
        Debug.Log($"Unit spawned: {unit.unitName} at position {spawnPoint.position}");
        
        // Update queue status
        uiManager.UpdateQueueStatus(productionQueue.Count, maxQueueSize);
        
        // Process the next unit in the queue
        ProcessProductionQueue();
    }
    
    public void CancelProduction()
    {
        if (isProducing)
        {
            StopAllCoroutines();
            isProducing = false;
            
            // Only cancel the current unit being produced
            if (productionQueue.Count > 0)
            {
                int cancelledUnitIndex = productionQueue.Dequeue();
                Debug.Log($"Cancelled production of {availableUnits[cancelledUnitIndex].unitName}");
            }
            
            uiManager.UpdateQueueStatus(productionQueue.Count, maxQueueSize);
            uiManager.UpdateProgress(0, "Cancelled");
            
            // Process the next unit in the queue
            ProcessProductionQueue();
        }
    }
    
    public void ClearQueue()
    {
        StopAllCoroutines();
        productionQueue.Clear();
        isProducing = false;
        
        uiManager.UpdateQueueStatus(0, maxQueueSize);
        uiManager.UpdateProgress(0, "Queue Cleared");
        
        Debug.Log("Production queue cleared");
    }
    
    private void OnDestroy()
    {
        // Ensure coroutines are stopped if building is destroyed
        StopAllCoroutines();
    }
    
    // Add this to your Building class
    public int[] GetQueuedUnitIndices()
    {
        return productionQueue.ToArray();
    }
}