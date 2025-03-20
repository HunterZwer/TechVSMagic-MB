using System;
using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public enum UnitClass { Knight, Archer, Shaman, Brute }
    public UnitClass unitClass;
    public Sprite unitIcon;

    [SerializeField] private float _unitHealth;
    public float unitMaxHealth;

    private Animator _animator;
    private UnitMovement _movement;
    public HealthTracker healthTracker;

    public bool IsDead { get; private set; } = false;
    private static readonly int DeadTrigger = Animator.StringToHash("Dead");

    public static event Action onUnitStatsChanged;
    public static event Action onUnitClassChanged;

    public static event Action<Unit> onUnitDied;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _movement = GetComponent<UnitMovement>();
    }

    private void Start()
    {
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
    }

    private IEnumerator DeathCoroutine()
    {
        yield return new WaitForSeconds(3.2f);
        Destroy(gameObject);
    }

   

    private void UpdateHealthUI(bool invokeEvent = true)
    {
        if (healthTracker != null)
        {
            healthTracker.UpdateSliderValue(_unitHealth, unitMaxHealth);
        }

        if (invokeEvent)
        {
            onUnitStatsChanged?.Invoke(); 
        }

        if (_unitHealth <= 0 && !IsDead)
        {
            Die();
        }
    }

   

    private void Die()
    {
        IsDead = true;

        // Оповещаем подписчиков о смерти юнита
        onUnitDied?.Invoke(this);

        // Останавливаем движение
        if (_movement != null && _movement.agent != null)
        {
            _movement.agent.isStopped = true;
            _movement.isCommandToMove = false;
        }

        // Анимация смерти
        if (_animator != null)
        {
            _animator.SetTrigger(DeadTrigger);
        }

        // Удаление юнита через корутину
        StartCoroutine(DeathCoroutine());
    }

    public void TakeDamage(int damageToInflict)
    {
        if (IsDead) return;

        _unitHealth = Mathf.Max(0, _unitHealth - damageToInflict);
        UpdateHealthUI();
    }

    public float GetCurrentHealth()
    {
        return _unitHealth;
    }

    public Sprite GetUnitIcon()
    {
        return unitIcon;
    }

    public void SetUnitClass(UnitClass newClass, Sprite newIcon)
    {
        unitClass = newClass;
        unitIcon = newIcon;
        onUnitClassChanged?.Invoke();
    }
}
