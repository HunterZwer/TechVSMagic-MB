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
        public int goldCost;
        public int silverCost;
    }
    
    [SerializeField] private UnitData[] availableUnits; 
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private BuildingUI uiManager;
    [SerializeField] private int maxQueueSize = 5;
    [SerializeField] float spawnRadius = 2f;

    private bool isProducing = false;
    private Queue<int> productionQueue = new Queue<int>();
    public event System.Action<int> OnUnitProduced;

    public int QueueCount => productionQueue.Count;
    public bool IsQueueFull => productionQueue.Count >= maxQueueSize;
    
    private void OnMouseDown()
    {
        uiManager.ShowForBuilding(this);
    }


    
    public string GetUnitName(int index)
    {
        if (index >= 0 && index < availableUnits.Length)
            return availableUnits[index].unitName;
        return "Unknown";
    }
    
    public bool StartProducingUnit(int unitIndex)
    {
        if (unitIndex < 0 || unitIndex >= availableUnits.Length || IsQueueFull)
            return false;

        UnitData unit = availableUnits[unitIndex];
        
        if (!EconomyManager.Instance.CanAfford(unit.goldCost, unit.silverCost))
            return false;

        EconomyManager.Instance.SpendResources(unit.goldCost, unit.silverCost);
        productionQueue.Enqueue(unitIndex);
        uiManager.UpdateQueueStatus(productionQueue.Count, maxQueueSize);

        if (!isProducing)
            ProcessProductionQueue();

        return true;
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
        int unitIndex = productionQueue.Peek();
        StartCoroutine(ProduceUnit(unitIndex));
    }
    
    private IEnumerator ProduceUnit(int unitIndex)
    {
        UnitData unit = availableUnits[unitIndex];
        float timer = 0;

        while (timer < unit.productionTime)
        {
            timer += Time.deltaTime;
            float progress = timer / unit.productionTime;
            uiManager.UpdateProgress(progress, unit.unitName);
            yield return null;
        }

        productionQueue.Dequeue();

        
       
        Vector3 randomOffset = new Vector3(
            Random.Range(-spawnRadius, spawnRadius),
            0,
            Random.Range(-spawnRadius, spawnRadius)
        );

        Vector3 spawnPosition = spawnPoint.position + randomOffset;

        Instantiate(unit.unitPrefab, spawnPosition, spawnPoint.rotation);

        uiManager.UpdateQueueStatus(productionQueue.Count, maxQueueSize);
        uiManager.RemoveCompletedUnitFromQueue(unitIndex);
        ProcessProductionQueue();
    }


    private void UnitProductionCompleted(int unitIndex)
    {
        OnUnitProduced?.Invoke(unitIndex);
    }

    public void CancelProduction()
    {
        if (isProducing)
        {
            StopAllCoroutines();
            isProducing = false;
            
            if (productionQueue.Count > 0)
            {
                int canceledIndex = productionQueue.Dequeue();
                RefundUnit(canceledIndex);
            }

            uiManager.UpdateQueueStatus(productionQueue.Count, maxQueueSize);
            uiManager.UpdateProgress(0, "Cancelled");
            ProcessProductionQueue();
        }
    }
    
    public void ClearQueue()
    {
        StopAllCoroutines();
        
        foreach (int index in productionQueue)
            RefundUnit(index);
        
        productionQueue.Clear();
        isProducing = false;
        
        if (uiManager != null)
        {
            uiManager.UpdateQueueStatus(0, maxQueueSize);
            uiManager.UpdateProgress(0, "Queue Cleared");
        }
    }

    public void RemoveUnitFromQueue(int unitIndex)
    {
        Queue<int> newQueue = new Queue<int>();
        bool removed = false;

        foreach (int queuedIndex in productionQueue)
        {
            if (queuedIndex == unitIndex && !removed)
            {
                RefundUnit(queuedIndex);
                removed = true;
                continue;
            }
            newQueue.Enqueue(queuedIndex);
        }

        productionQueue = newQueue;
        uiManager.UpdateQueueStatus(productionQueue.Count, maxQueueSize);
    }

    private void RefundUnit(int unitIndex)
    {
        UnitData unit = availableUnits[unitIndex];
        EconomyManager.Instance.AddResources(unit.goldCost, unit.silverCost);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
