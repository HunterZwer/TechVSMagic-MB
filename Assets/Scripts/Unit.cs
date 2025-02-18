using System;
using UnityEngine;

public class Unit : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private float _unitHealth;
    public float unitMaxHealth;

    public HealthTracker healthTracker;
    void Start()
    {
        UnitSelectionManager.Instance.allUnitSelected.Add(this.gameObject);

        _unitHealth = unitMaxHealth;
    }

    private void OnDestroy()
    {
        UnitSelectionManager.Instance.allUnitSelected.Remove(this.gameObject);
    }

    private void UpdateHealthUI()
    {
        healthTracker.UpdateSliderValue(_unitHealth, unitMaxHealth);

        if (_unitHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    internal void TakeDamage(int damageToInflict)
    {
        _unitHealth -= damageToInflict;
        UpdateHealthUI();
    }
}
