using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UpgradeBuildingUI : MonoBehaviour
{
    [SerializeField] private GameObject upgradeUI;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button clearQueueButton;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Text progressText;
    [SerializeField] private Text queueText;
    [SerializeField] private Transform queueGrid;
    [SerializeField] private GameObject queueButtonPrefab;

    [Header("Upgrade Buttons")]
    [SerializeField] private Button[] upgradeButtons;
    [SerializeField] private UpgradeData[] upgradeDataList;

    private List<GameObject> queueButtons = new List<GameObject>();
    private Queue<UpgradeData> upgradeQueue = new Queue<UpgradeData>();
    private bool isProducing = false;
    private UpgradeData currentUpgrade;
    
    private HashSet<string> activeUpgradeNames = new HashSet<string>();
    private CanvasGroup canvasGroup;
    

    private void Start()
    {
        
        upgradeUI.SetActive(false);
        canvasGroup = GetComponent<CanvasGroup>();

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            int index = i;
            upgradeButtons[i].onClick.AddListener(() =>
            {
                if (StartUpgrade(upgradeDataList[index]))
                {
                    AddToQueueVisual(upgradeDataList[index].upgradeName);
                }
            });
        }

        cancelButton.onClick.AddListener(CancelCurrentUpgrade);
        clearQueueButton.onClick.AddListener(ClearUpgradeQueue);
    }

    public void ShowUI()
    {
        upgradeUI.SetActive(true);
        UpdateQueueText();
        canvasGroup.alpha = 1f; // Fully transparent
        canvasGroup.interactable = true; // Blocks input
        canvasGroup.blocksRaycasts = true; // Let clicks go through
    }

    public void HideUI()
    {
        canvasGroup.alpha = 0f; // Fully transparent
        canvasGroup.interactable = false; // Blocks input
        canvasGroup.blocksRaycasts = false; // Let clicks go through

    }

    private bool StartUpgrade(UpgradeData data)
    {
        if (activeUpgradeNames.Contains(data.upgradeName))
        {
            Debug.Log($"Upgrade '{data.upgradeName}' is already in progress or queued.");
            return false;
        }
        int currentLevel = Upgrader.Instance.GetUpgradeLevel(data.upgradeName);
        int goldCost = Mathf.RoundToInt(data.baseGoldCost * Mathf.Pow(data.costMultiplier, currentLevel));
        int silverCost = Mathf.RoundToInt(data.baseSilverCost * Mathf.Pow(data.costMultiplier, currentLevel));

        if (!EconomyManager.Instance.CanAfford(goldCost, silverCost))
            return false;

        EconomyManager.Instance.SpendResources(goldCost, silverCost);
        upgradeQueue.Enqueue(data);
        activeUpgradeNames.Add(data.upgradeName);

        if (!isProducing)
            StartCoroutine(ProduceUpgrade());

        return true;
    }

    private IEnumerator<WaitForEndOfFrame> ProduceUpgrade()
    {
        isProducing = true;
        currentUpgrade = upgradeQueue.Peek();

        float timer = 0f;
        while (timer < currentUpgrade.productionTime)
        {
            timer += Time.deltaTime;
            float progress = timer / currentUpgrade.productionTime;
            progressBar.value = progress;
            progressText.text = $"Upgrading {currentUpgrade.upgradeName}: {(progress * 100):0}%";
            yield return new WaitForEndOfFrame();
        }

        Upgrader.Instance.ApplyUpgrade(currentUpgrade.upgradeName);
        upgradeQueue.Dequeue();
        RemoveFromQueueVisual();
        activeUpgradeNames.Remove(currentUpgrade.upgradeName);
        isProducing = false;

        if (upgradeQueue.Count > 0)
            StartCoroutine(ProduceUpgrade());
        else
        {
            progressText.text = "Idle";
            progressBar.value = 0f;
        }

    }

    private void CancelCurrentUpgrade()
    {
        if (!isProducing) return;

        StopAllCoroutines();
        RefundUpgrade(currentUpgrade);
        upgradeQueue.Dequeue();
        RemoveFromQueueVisual();
        progressText.text = "Cancelled";
        progressBar.value = 0;
        isProducing = false;

        if (upgradeQueue.Count > 0)
            StartCoroutine(ProduceUpgrade());
    }

    private void ClearUpgradeQueue()
    {
        StopAllCoroutines();
        foreach (UpgradeData upgrade in upgradeQueue)
        {
            RefundUpgrade(upgrade);
        }

        upgradeQueue.Clear();
        ClearQueueVisuals();
        progressText.text = "Queue Cleared";
        progressBar.value = 0f;
        isProducing = false;
    }

    private void RefundUpgrade(UpgradeData data)
    {
        int currentLevel = Upgrader.Instance.GetUpgradeLevel(data.upgradeName);
        int goldCost = Mathf.RoundToInt(data.baseGoldCost * Mathf.Pow(data.costMultiplier, currentLevel));
        int silverCost = Mathf.RoundToInt(data.baseSilverCost * Mathf.Pow(data.costMultiplier, currentLevel));

        EconomyManager.Instance.AddResources(goldCost, silverCost);
    }

    private void AddToQueueVisual(string upgradeName)
    {
        GameObject queueBtn = Instantiate(queueButtonPrefab, queueGrid);
    
        // Set text
        Text queueText = queueBtn.GetComponentInChildren<Text>();
        if (queueText != null)
            queueText.text = upgradeName;

        // Set icon
        Image queueIcon = queueBtn.GetComponentInChildren<Image>();
        Sprite sourceSprite = GetUpgradeButtonSprite(upgradeName);
        if (queueIcon != null && sourceSprite != null)
            queueIcon.sprite = sourceSprite;

        queueButtons.Add(queueBtn);
        UpdateQueueText();
    }
    
    private Sprite GetUpgradeButtonSprite(string upgradeName)
    {
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (upgradeDataList[i].upgradeName == upgradeName)
            {
                Image icon = upgradeButtons[i].GetComponentInChildren<Image>();
                return icon != null ? icon.sprite : null;
            }
        }
        return null;
    }



    private void RemoveFromQueueVisual()
    {
        if (queueButtons.Count > 0)
        {
            Destroy(queueButtons[0]);
            queueButtons.RemoveAt(0);
        }
        UpdateQueueText();
    }

    private void ClearQueueVisuals()
    {
        foreach (var btn in queueButtons)
            Destroy(btn);

        queueButtons.Clear();
        UpdateQueueText();
    }

    private void UpdateQueueText()
    {
        queueText.text = $"Queue: {queueButtons.Count}";
        queueText.color = (queueButtons.Count > 0) ? Color.yellow : Color.white;
    }
}
