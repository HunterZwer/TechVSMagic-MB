using System;
using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public enum UnitClass { Swordsman, Archer, Shaman, Brute } // Классы юнитов
    public UnitClass unitClass;
    public Sprite unitIcon; // Иконка юнита

    private float _unitHealth;
    public float unitMaxHealth;
    private Animator animator;

    public HealthTracker healthTracker;

    private bool isDead = false; // Флаг для предотвращения повторного вызова смерти

    // События для обновления UI
    public static event Action onUnitStatsChanged;
    public static event Action onUnitClassChanged;

    private void Start()
    {
        animator = GetComponent<Animator>();
        UnitSelectionManager.Instance.allUnitSelected.Add(this.gameObject);
        _unitHealth = unitMaxHealth;
    }

    private IEnumerator DeathAnimationHolder()
    {
        UnitSelectionManager.Instance.allUnitSelected.Remove(this.gameObject);
        yield return new WaitForSeconds(3.2f);
        Destroy(gameObject);
    }

    private void UpdateHealthUI()
    {
        healthTracker.UpdateSliderValue(_unitHealth, unitMaxHealth);

        if (_unitHealth <= 0 && !isDead)
        {
            isDead = true; // Устанавливаем флаг
            animator.SetTrigger("Dead");
            StartCoroutine(DeathAnimationHolder());
        }

        onUnitStatsChanged?.Invoke(); // Обновление UI при изменении здоровья
    }

    internal void TakeDamage(int damageToInflict)
    {
        if (isDead) return; // Если юнит уже мертв, не продолжаем

        _unitHealth -= damageToInflict;
        _unitHealth = Mathf.Max(0, _unitHealth); // Защита от отрицательных значений
        UpdateHealthUI();
    }

    // Геттер текущего здоровья
    public float GetCurrentHealth()
    {
        return _unitHealth;
    }

    // Геттер иконки юнита
    public Sprite GetUnitIcon()
    {
        return unitIcon;
    }

    // Метод для изменения класса юнита (если нужно)
    public void SetUnitClass(UnitClass newClass, Sprite newIcon)
    {
        unitClass = newClass;
        unitIcon = newIcon;
        onUnitClassChanged?.Invoke(); // Уведомляем UI
    }
}
