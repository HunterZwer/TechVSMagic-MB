using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class UnitLVL2 : MonoBehaviour
{
    public enum UnitClass { Knight, Archer, Shaman, Brute }
    public UnitClass unitClass;
    public bool IsPlayer;    
    public Sprite unitIcon;
    public HealthTracker healthTracker;
    public Transform circleIndicator;
    public float unitMaxHealth;
    private int _baseHealth = 100;
    public bool IsDead { get; private set; } = false;
    [SerializeField] private float _unitHealth;
    private UnitStats unitStats;
    private Animator _animator;
    private UnitMovement _movement;
    private int _healthUprgadeLevel = 0;
    public string InGameName;
    
    private static readonly int DeadTrigger = Animator.StringToHash("Dead");
    public static event Action onUnitStatsChanged;
    public static event Action onUnitClassChanged;
    public static event Action<UnitLVL2> onUnitDied;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _movement = GetComponent<UnitMovement>();
        circleIndicator = transform.Find("CircleIndicator");

        var path = "Assets/Resources/Config/Units/Names&Surnames.txt";
        if (this.unitClass == UnitClass.Archer) { path = "Assets/Resources/Config/Units/FemaleNames&Surnames.txt";}
        var FileNames = File.ReadAllLines(path); // Read all lines into an array
        var FileNamesLenHalf = FileNames.Length / 2;
        InGameName = FileNames[UnityEngine.Random.Range(0, FileNamesLenHalf)] + " " + 
                     FileNames[UnityEngine.Random.Range(FileNamesLenHalf, FileNames.Length)];
    }

    private void Start()
    {
        if (IsPlayer)
        {
            UnitRegistryManager.RegisterPlayerUnit(gameObject);
        }
        unitStats = JsonLoader.LoadUnitStats(unitClass, IsPlayer);
        _animator.cullingMode = AnimatorCullingMode.CullCompletely;
        unitMaxHealth = _baseHealth;
        if (IsPlayer && Upgrader.Instance is not null)
        {
            if (unitClass is UnitClass.Archer or UnitClass.Shaman)
            {
                _healthUprgadeLevel = Upgrader.Instance.healthRangedUpgradeLevel;
            }
            else
            {
                _healthUprgadeLevel = Upgrader.Instance.healthMeleeUpgradeLevel;
            }
            unitMaxHealth *=  unitStats.HealthMultiplier[_healthUprgadeLevel];
        }
        else
        {
            unitMaxHealth *=  unitStats.HealthMultiplier[0];
        }
        if (UnitSelectionManager.Instance != null)
        {
            UnitSelectionManager.Instance.allUnitSelected.Add(gameObject);
        }
        
        _unitHealth = unitMaxHealth;
        UpdateHealthUI(false);
    }

    public virtual void ApplyHealthUpgrade()
    {
        if (!IsPlayer) return;
        if (unitClass is UnitClass.Archer or UnitClass.Shaman)
        {
            unitMaxHealth = _baseHealth * unitStats.DamageMultiplier[Upgrader.Instance.healthRangedUpgradeLevel];
        }
        else
        {
            unitMaxHealth = _baseHealth * unitStats.DamageMultiplier[Upgrader.Instance.healthMeleeUpgradeLevel];
        }
    }
    
    private void OnDestroy()
    {
        if (UnitSelectionManager.Instance != null)
        {
            UnitSelectionManager.Instance.allUnitSelected.Remove(gameObject);
        }
        if (IsPlayer) UnitRegistryManager.UnregisterPlayerUnit(gameObject);
    }
    
    public void SetUnitClass(UnitClass newClass, Sprite newIcon)
    {
        unitClass = newClass;
        unitIcon = newIcon;
        onUnitClassChanged?.Invoke();
    }

    private void UpdateHealthUI(bool invokeEvent = true)
    {
        if (IsDead) return;
        if (healthTracker) healthTracker.UpdateSliderValue(_unitHealth, unitMaxHealth);
        if (invokeEvent && onUnitStatsChanged != null)
            onUnitStatsChanged.Invoke();
        if (_unitHealth <= 0) Die();
    }
    
    public float GetCurrentHealth() => _unitHealth;
    public Sprite GetUnitIcon() => unitIcon;
    
    public void TakeDamage(float damageToInflict)
    {
        if (IsDead) return;
        _unitHealth = Mathf.Max(0, _unitHealth - damageToInflict);
        UpdateHealthUI();
    }

    private void Die()
    {
        // Оповещаем подписчиков о смерти юнита
        onUnitDied?.Invoke(this);
        IsDead = true;
        
        // Останавливаем движение
        if (_movement.agent) 
        { 
            _movement.agent.isStopped = true;
            _movement.isCommandToMove = false; 
        }
        if (_animator) _animator.SetTrigger(DeadTrigger);
        Destroy(gameObject, 3.2f);;
    }

}
