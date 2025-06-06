using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class Upgrader : MonoBehaviour
{
    public int damageUpgradeLevel;
    public int healthMeleeUpgradeLevel;
    public int healthRangedUpgradeLevel;
    public int rangedDamageUpgradeLevel;
    public int speedUpgradeLevel;
    public int rangeDamageUpgradeLevel;
    public int rangeUpgradeLevel;
    
    private int updatesAmount = 4;
    private int silverUpgradeLevel = 0;
    private int goldUpgradeLevel = 0;
    private int moneyUpgradeAmount = 11;
    
    public static Upgrader Instance { get; private set; }
    
    [SerializeField] private Button meleedamageUpgradeButton;
    [SerializeField] private Sprite[] meleedamageUpgradeSprites;
    private Image meleedamageUpgradeImage;
    
    [SerializeField] private Button rangedDamageUpgradeButton;
    [SerializeField] private Sprite[] rangedDamageUpgradeSprites;
    private Image rangedDamageUpgradeImage;
    
    [SerializeField] private Button rangeUpgradeButton;
    [SerializeField] private Sprite[] rangeUpgradeSprites;
    private Image rangeUpgradeImage;
    
    [SerializeField] private Button healthMeleeUpgradeButton;
    [SerializeField] private Sprite[] healthMeleeUpgradeSprites;
    private Image healthMeleeUpgradeImage;    
    
    [SerializeField] private Button healthRangedUpgradeButton;
    [SerializeField] private Sprite[] healthRangedUpgradeSprites;
    private Image healthRangedUpgradeImage;
    
    [SerializeField] private Button speedUpgradeButton;
    [SerializeField] private Sprite[] speedUpgradeSprites;
    private Image speedUpgradeImage;
    
    [SerializeField] private Button GoldUpgradeButton;
    [SerializeField] private Sprite[] GoldUpgradeSprites;
    [SerializeField] private float goldUpgradePercent = 0.3f;
    private Image goldUpgradeImage;

    [SerializeField] private Button SilverUpgradeButton;
    [SerializeField] private Sprite[] SilverUpgradeSprites;
    [SerializeField] private float silverUpgradePercent = 0.3f;
    private Image silverUpgradeImage;
    
    
    private Queue<UpgradeData> upgradeQueue = new Queue<UpgradeData>();
    private bool isProducingUpgrade = false;
    private UpgradeData currentUpgrade;
    
    
    public void ApplyUpgrade(string upgradeName)
    {
        switch (upgradeName)
        {
            case "Damage": UpgradeDamage(); break;
            case "HealthMelee": UpgradeMeleeHealth(); break;
            case "HealthRanged": UpgradeRangedHealth(); break;
            case "RangedDamage": UpgradeRangedDamage(); break;
            case "Speed": UpgradeSpeed(); break;
            case "Range": UpgradeRange(); break;
            case "Gold": UpgradeGold(); break;
            case "Silver": UpgradeSilver(); break;
            default:
                Debug.LogWarning("Unknown upgrade applied: " + upgradeName);
                break;
        }
    }

    
    public int GetUpgradeLevel(string upgradeName)
    {
        switch (upgradeName)
        {
            case "Damage": return damageUpgradeLevel;
            case "HealthMelee": return healthMeleeUpgradeLevel;
            case "HealthRanged": return healthRangedUpgradeLevel;
            case "RangedDamage": return rangedDamageUpgradeLevel;
            case "Speed": return speedUpgradeLevel;
            case "Range": return rangeUpgradeLevel;
            case "Gold": return goldUpgradeLevel;
            case "Silver": return silverUpgradeLevel;
            default:
                Debug.LogWarning($"Unknown upgrade name: {upgradeName}");
                return 0;
        }
    }
    
    public bool StartUpgrade(UpgradeData data)
    {
        int level = GetUpgradeLevel(data.upgradeName);
        int goldCost = Mathf.RoundToInt(data.baseGoldCost * Mathf.Pow(data.costMultiplier, level));
        int silverCost = Mathf.RoundToInt(data.baseSilverCost * Mathf.Pow(data.costMultiplier, level));

        if (!EconomyManager.Instance.CanAfford(goldCost, silverCost))
            return false;

        EconomyManager.Instance.SpendResources(goldCost, silverCost);
        upgradeQueue.Enqueue(data);

        if (!isProducingUpgrade)
            StartCoroutine(ProcessUpgradeQueue());

        return true;
    }
    
    private IEnumerator ProcessUpgradeQueue()
    {
        if (upgradeQueue.Count == 0) yield break;

        isProducingUpgrade = true;
        currentUpgrade = upgradeQueue.Peek();

        float timer = 0f;
        while (timer < currentUpgrade.productionTime)
        {
            timer += Time.deltaTime;
            // Optional: Notify UI of progress here
            yield return null;
        }

        ApplyUpgrade(currentUpgrade.upgradeName);
        upgradeQueue.Dequeue();
        isProducingUpgrade = false;

        if (upgradeQueue.Count > 0)
            StartCoroutine(ProcessUpgradeQueue());
    }
    
    

    private void Awake()
    {
        damageUpgradeLevel = 0;
        healthRangedUpgradeLevel = 0;
        healthMeleeUpgradeLevel = 0;
        rangedDamageUpgradeLevel = 0;
        speedUpgradeLevel = 0;
        rangeDamageUpgradeLevel = 0;
        rangeUpgradeLevel = 0;
        
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
        meleedamageUpgradeImage = meleedamageUpgradeButton.GetComponent<Image>();
        goldUpgradeImage = GoldUpgradeButton.GetComponent<Image>();
        silverUpgradeImage = SilverUpgradeButton.GetComponent<Image>();
        rangedDamageUpgradeImage = rangedDamageUpgradeButton.GetComponent<Image>();
        rangeUpgradeImage = rangeUpgradeButton.GetComponent<Image>();
        healthMeleeUpgradeImage = healthMeleeUpgradeButton.GetComponent<Image>();
        healthRangedUpgradeImage = healthRangedUpgradeButton.GetComponent<Image>();
        speedUpgradeImage = speedUpgradeButton.GetComponent<Image>();
    }
    

    public void UpgradeDamage()
    {
        if (damageUpgradeLevel + 1 >= updatesAmount)
        {
            Destroy(meleedamageUpgradeButton.gameObject);
            return;
        }

        damageUpgradeLevel++;
        if (damageUpgradeLevel < meleedamageUpgradeSprites.Length)
        {
            meleedamageUpgradeImage.sprite = meleedamageUpgradeSprites[damageUpgradeLevel];
        }
        if (UnitRegistryManager.ReturnAllPlayerUnits().Count == 0){return;}
        foreach (var unit in UnitRegistryManager.ReturnAllPlayerUnits())
        {
            unit.TryGetComponent(out MeleeAttackController meleeAttackController);
            meleeAttackController?.ApplyDamageUpgrade();
        }
    }        
    
    public void UpgradeRangedDamage()
    {
        if (rangedDamageUpgradeLevel + 1 >= updatesAmount)
        {
            Destroy(rangedDamageUpgradeButton.gameObject);
            return;
        }
        if (rangedDamageUpgradeLevel < rangedDamageUpgradeSprites.Length)
        {
            rangedDamageUpgradeImage.sprite = rangedDamageUpgradeSprites[rangedDamageUpgradeLevel];
        }
        rangedDamageUpgradeLevel++;
        if (UnitRegistryManager.ReturnAllPlayerUnits().Count == 0){return;}
        foreach (var unit in UnitRegistryManager.ReturnAllPlayerUnits())
        {
            unit.TryGetComponent(out RangeAttackController rangeAttackController);
            rangeAttackController?.ApplyRangedDamageUpgrade();
        }
    }    
    
    public void UpgradeRangedHealth()
    {
        if (healthRangedUpgradeLevel + 1 >= updatesAmount)
        {
            Destroy(healthRangedUpgradeButton.gameObject);
            return;
        }

        healthRangedUpgradeLevel++;
        if (healthRangedUpgradeLevel < healthRangedUpgradeSprites.Length)
        {
            healthRangedUpgradeImage.sprite = healthRangedUpgradeSprites[healthRangedUpgradeLevel];
        }
        if (UnitRegistryManager.ReturnAllPlayerUnits().Count == 0){return;}
        foreach (var unit in UnitRegistryManager.ReturnAllPlayerUnits())
        {
            unit.TryGetComponent(out UnitLVL2 _unitComponent);
            if (_unitComponent.unitClass is UnitLVL2.UnitClass.Archer or UnitLVL2.UnitClass.Shaman)
            { _unitComponent.ApplyHealthUpgrade();}
        }
        Debug.Log(UnitRegistryManager.ReturnAllPlayerUnits().Count);
    }

    public void UpgradeMeleeHealth()
    {
        if (healthMeleeUpgradeLevel + 1 >= updatesAmount)
        {
            Destroy(healthMeleeUpgradeButton.gameObject);
            return;
        }

        healthMeleeUpgradeLevel++;
        if (healthMeleeUpgradeLevel < healthMeleeUpgradeSprites.Length)
        {
            healthMeleeUpgradeImage.sprite = healthMeleeUpgradeSprites[healthMeleeUpgradeLevel];
        }
        if (UnitRegistryManager.ReturnAllPlayerUnits().Count == 0){return;}
        foreach (var unit in UnitRegistryManager.ReturnAllPlayerUnits())
        {
            unit.TryGetComponent(out UnitLVL2 _unitComponent);
            if (_unitComponent.unitClass is UnitLVL2.UnitClass.Knight or UnitLVL2.UnitClass.Brute)
            {
                _unitComponent.ApplyHealthUpgrade();
            }
        }
    }    
    
    public void UpgradeSpeed()
    {
        if (speedUpgradeLevel + 1 >= updatesAmount)
        {
            Destroy(speedUpgradeButton.gameObject);
            return;
        }
        
        if (speedUpgradeLevel < speedUpgradeSprites.Length)
        {
            speedUpgradeImage.sprite = speedUpgradeSprites[speedUpgradeLevel];
        }

        speedUpgradeLevel++;
        if (UnitRegistryManager.ReturnAllPlayerUnits().Count == 0){return;}
        foreach (var unit in UnitRegistryManager.ReturnAllPlayerUnits())
        {
            unit.TryGetComponent(out UnitMovement unitMovement);
            unitMovement.ApplySpeedeUpgrade();
        }
    }


    public void UpgradeRange()
    {
        if (rangeUpgradeLevel + 1 >= updatesAmount)
        {
        Destroy(rangeUpgradeButton.gameObject);
        return;
        }

        rangeUpgradeLevel++;
        if (rangeUpgradeLevel < rangeUpgradeSprites.Length)
        {
            rangeUpgradeImage.sprite = rangeUpgradeSprites[rangeUpgradeLevel];
        }
        if (UnitRegistryManager.ReturnAllPlayerUnits().Count == 0){return;}
        foreach (var unit in UnitRegistryManager.ReturnAllPlayerUnits())
        {
            unit.TryGetComponent(out RangeAttackController rangeAttackController);
            rangeAttackController?.ApplyRangedUpgrade();
        }
    }

    public void UpgradeGold()
    {
        if (goldUpgradeLevel + 1 >= moneyUpgradeAmount)
        {
            Destroy(GoldUpgradeButton.gameObject);
            return;
        }
        goldUpgradeLevel++;
        EconomyManager.Instance.goldPerSecond += Mathf.RoundToInt(EconomyManager.Instance.goldPerSecond * goldUpgradePercent);
        if (goldUpgradeLevel < GoldUpgradeSprites.Length)
        {
            goldUpgradeImage.sprite = GoldUpgradeSprites[goldUpgradeLevel];
        }
    }    
    public void UpgradeSilver()
    {
        if (silverUpgradeLevel + 1 >= moneyUpgradeAmount)
        {
            Destroy(SilverUpgradeButton.gameObject);
            return;
        }
        silverUpgradeLevel++;
        EconomyManager.Instance.silverPerSecond += Mathf.RoundToInt(EconomyManager.Instance.silverPerSecond * silverUpgradePercent);
        if (silverUpgradeLevel < SilverUpgradeSprites.Length)
        {
            silverUpgradeImage.sprite = SilverUpgradeSprites[silverUpgradeLevel];
        }
    }

    private void OnDestroy()
    {
        FloatingTextManager.ShowText3("You lose");
    }
    
}