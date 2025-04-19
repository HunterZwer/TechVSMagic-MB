using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class Upgrader : MonoBehaviour
{
    public int damageUpgradeLevel;
    public int healthUpgradeLevel;
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
    
    [SerializeField] private Button healthUpgradeButton;
    [SerializeField] private Sprite[] healthUpgradeSprites;
    private Image healthUpgradeImage;
    
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
    
    private void Start()
    {
        meleedamageUpgradeButton.onClick.AddListener(() => {Upgrader.Instance.UpgradeDamage(); });
        healthUpgradeButton.onClick.AddListener(() => {Upgrader.Instance.UpgradeHealth(); });
        rangeUpgradeButton.onClick.AddListener(() => {Upgrader.Instance.UpgradeRange(); });
        rangedDamageUpgradeButton.onClick.AddListener(() => {Upgrader.Instance.UpgradeRangedDamage(); });
        speedUpgradeButton.onClick.AddListener(() => {Upgrader.Instance.UpgradeSpeed(); });
        SilverUpgradeButton.onClick.AddListener(() => {Upgrader.Instance.UpgradeSilver(); });
        GoldUpgradeButton.onClick.AddListener(() => {Upgrader.Instance.UpgradeGold(); });
    }


    private void Awake()
    {
        damageUpgradeLevel = 0;
        healthUpgradeLevel = 0;
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
        healthUpgradeImage = healthUpgradeButton.GetComponent<Image>();
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
        if (UnitRegistryManager.ReturnAllPlayerUnits().Count != 0){return;}
        foreach (var unit in UnitRegistryManager.ReturnAllPlayerUnits())
        {
            unit.TryGetComponent(out MeleeAttackController meleeAttackController);
            meleeAttackController?.ApplyDamageUpgrade();
        }
        
        if (damageUpgradeLevel < meleedamageUpgradeSprites.Length)
        {
            meleedamageUpgradeImage.sprite = meleedamageUpgradeSprites[damageUpgradeLevel];
        }
    }        
    
    public void UpgradeRangedDamage()
    {
        if (rangedDamageUpgradeLevel + 1 >= updatesAmount)
        {
            Destroy(rangedDamageUpgradeButton.gameObject);
            return;
        }

        rangedDamageUpgradeLevel++;
        if (UnitRegistryManager.ReturnAllPlayerUnits().Count != 0){return;}
        foreach (var unit in UnitRegistryManager.ReturnAllPlayerUnits())
        {
            unit.TryGetComponent(out RangeAttackController rangeAttackController);
            rangeAttackController?.ApplyRangedDamageUpgrade();
        }
        
        if (rangedDamageUpgradeLevel < rangedDamageUpgradeSprites.Length)
        {
            rangedDamageUpgradeImage.sprite = rangedDamageUpgradeSprites[rangedDamageUpgradeLevel];
        }
    }    
    
    public void UpgradeHealth()
    {
        if (healthUpgradeLevel + 1 >= updatesAmount)
        {
            Destroy(healthUpgradeButton.gameObject);
            return;
        }

        healthUpgradeLevel++;
        if (UnitRegistryManager.ReturnAllPlayerUnits().Count != 0){return;}
        foreach (var unit in UnitRegistryManager.ReturnAllPlayerUnits())
        {
            unit.TryGetComponent(out Unit _unitComponent);
            if (_unitComponent.IsPlayer)
            {
                _unitComponent.ApplyHealthUpgrade();
            }
        }
        
        if (healthUpgradeLevel < healthUpgradeSprites.Length)
        {
            healthUpgradeImage.sprite = healthUpgradeSprites[healthUpgradeLevel];
        }
    }    
    
    public void UpgradeSpeed()
    {
        if (speedUpgradeLevel + 1 >= updatesAmount)
        {
            Destroy(speedUpgradeButton.gameObject);
            return;
        }

        speedUpgradeLevel++;
        if (UnitRegistryManager.ReturnAllPlayerUnits().Count != 0){return;}
        foreach (var unit in UnitRegistryManager.ReturnAllPlayerUnits())
        {
            unit.TryGetComponent(out UnitMovement unitMovement);
            unitMovement.ApplySpeedeUpgrade();
        }
        
        if (speedUpgradeLevel < speedUpgradeSprites.Length)
        {
            speedUpgradeImage.sprite = speedUpgradeSprites[speedUpgradeLevel];
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
        if (UnitRegistryManager.ReturnAllPlayerUnits().Count != 0){return;}
        foreach (var unit in UnitRegistryManager.ReturnAllPlayerUnits())
        {
            unit.TryGetComponent(out RangeAttackController rangeAttackController);
            rangeAttackController?.ApplyRangedUpgrade();
        }
        
        if (rangeUpgradeLevel < rangeUpgradeSprites.Length)
        {
            rangeUpgradeImage.sprite = rangeUpgradeSprites[rangeUpgradeLevel];
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
        if (UnitRegistryManager.ReturnAllPlayerUnits().Count != 0){return;}
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
        if (UnitRegistryManager.ReturnAllPlayerUnits().Count != 0){return;}
        EconomyManager.Instance.silverPerSecond += Mathf.RoundToInt(EconomyManager.Instance.silverPerSecond * silverUpgradePercent);
        if (silverUpgradeLevel < SilverUpgradeSprites.Length)
        {
            silverUpgradeImage.sprite = SilverUpgradeSprites[silverUpgradeLevel];
        }
    }

    
}