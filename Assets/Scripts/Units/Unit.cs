using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class Unit : MonoBehaviour
{
    public enum UnitClass { Knight, Archer, Shaman, Brute }
    public UnitClass unitClass;
    public bool IsPlayer;    
    public Sprite unitIcon;
    public HealthTracker healthTracker;
    public Transform circleIndicator;
    public float unitMaxHealth;
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
    public static event Action<Unit> onUnitDied;
    private static readonly Random _rand = new Random();

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _movement = GetComponent<UnitMovement>();
        circleIndicator = transform.Find("CircleIndicator");
        if (IsPlayer)
        {
            UnitSelectionManager.RegisterPlayerUnit(gameObject);
        }

        var path = "Assets/Resources/Config/Units/Names&Surnames.txt";
        if (this.unitClass == UnitClass.Archer) { path = "Assets/Resources/Config/Units/FemaleNames&Surnames.txt";}
        var FileNames = File.ReadAllLines(path); // Read all lines into an array
        var FileNamesLenHalf = FileNames.Length / 2;
        InGameName = FileNames[UnityEngine.Random.Range(0, FileNamesLenHalf)] + " " + 
                     FileNames[UnityEngine.Random.Range(FileNamesLenHalf, FileNames.Length)];
    }

    private void Start()
    {
        unitStats = JsonLoader.LoadUnitStats(unitClass, IsPlayer);
        _animator.cullingMode = AnimatorCullingMode.CullCompletely;
        unitMaxHealth = unitMaxHealth * unitStats.HealthMultiplier[_healthUprgadeLevel];
        if (UnitSelectionManager.Instance != null)
        {
            UnitSelectionManager.Instance.allUnitSelected.Add(gameObject);
        }
        _unitHealth = unitMaxHealth;
        UpdateHealthUI(false);
    }

    private void OnDestroy()
    {
        if (UnitSelectionManager.Instance != null)
        {
            UnitSelectionManager.Instance.allUnitSelected.Remove(gameObject);
        }
        if (IsPlayer) UnitSelectionManager.UnregisterPlayerUnit(gameObject);
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
