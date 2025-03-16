using System;
using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    // Unit type configuration
    public enum UnitClass { Swordsman, Archer, Shaman, Brute }
    public UnitClass unitClass;
    public Sprite unitIcon;
    
    // Health settings
    [SerializeField] private float _unitHealth;
    public float unitMaxHealth;
    
    // Component references
    private Animator _animator;
    private UnitMovement _movement;
    public HealthTracker healthTracker;
    
    // State tracking
    private bool _isDead = false;
    private static readonly int DeadTrigger = Animator.StringToHash("Dead");
    
    // Events
    public static event Action onUnitStatsChanged;
    public static event Action onUnitClassChanged;
    
    private void Awake()
    {
        // Cache components in Awake
        _animator = GetComponent<Animator>();
        _movement = GetComponent<UnitMovement>();
    }
    
    private void Start()
    {
        // Add to global registry and initialize health
        if (UnitSelectionManager.Instance != null)
        {
            UnitSelectionManager.Instance.allUnitSelected.Add(gameObject);
        }
        _unitHealth = unitMaxHealth;
        
        // Initialize health display
        UpdateHealthUI(false);
    }
    
    private void OnDestroy()
    {
        // Clean up references when destroyed
        if (UnitSelectionManager.Instance != null)
        {
            UnitSelectionManager.Instance.allUnitSelected.Remove(gameObject);
        }
    }
    
    private IEnumerator DeathCoroutine()
    {
        // More efficient than DeathAnimationHolder - removes from list only once
        yield return new WaitForSeconds(3.2f);
        Destroy(gameObject);
    }
    
    private void UpdateHealthUI(bool invokeEvent = true)
    {
        // Only update if tracker exists
        if (healthTracker != null)
        {
            healthTracker.UpdateSliderValue(_unitHealth, unitMaxHealth);
        }
        
        // Only invoke event when needed
        if (invokeEvent)
        {
            onUnitStatsChanged?.Invoke();
        }
        
        // Check for death
        if (_unitHealth <= 0 && !_isDead)
        {
            Die();
        }
    }
    
    private void Die()
    {
        _isDead = true;
        
        // Stop movement
        if (_movement != null && _movement.agent != null)
        {
            _movement.agent.isStopped = true;
            _movement.isCommandToMove = false;
        }
        
        // Trigger death animation
        if (_animator != null)
        {
            _animator.SetTrigger(DeadTrigger);
        }
        
        // Start death sequence
        StartCoroutine(DeathCoroutine());
    }
    
    public void TakeDamage(int damageToInflict)
    {
        // Early exit if already dead
        if (_isDead) return;
        
        // Apply damage with bounds check
        _unitHealth = Mathf.Max(0, _unitHealth - damageToInflict);
        UpdateHealthUI();
    }
    
    // Getter for current health
    public float GetCurrentHealth()
    {
        return _unitHealth;
    }
    
    // Getter for unit icon
    public Sprite GetUnitIcon()
    {
        return unitIcon;
    }
    
    // Method to change unit class
    public void SetUnitClass(UnitClass newClass, Sprite newIcon)
    {
        unitClass = newClass;
        unitIcon = newIcon;
        onUnitClassChanged?.Invoke();
    }
}