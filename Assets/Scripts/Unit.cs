using System;
using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private float _unitHealth;
    public float unitMaxHealth;
    private Animator animator;

    public HealthTracker healthTracker;
    private void Start()
    {
        animator = GetComponent<Animator>();

        UnitSelectionManager.Instance.allUnitSelected.Add(this.gameObject);


        _unitHealth = unitMaxHealth;
    }


    private IEnumerator DeathAnimationHolder() {
        animator.SetTrigger("Dead");
        UnitSelectionManager.Instance.allUnitSelected.Remove(this.gameObject);
        yield return new WaitForSeconds(3.2f);
        Destroy(gameObject);
       
    }


    private void UpdateHealthUI()
    {
        healthTracker.UpdateSliderValue(_unitHealth, unitMaxHealth);

        if (_unitHealth <= 0)
        {
            StartCoroutine(DeathAnimationHolder());
        }
    }

    internal void TakeDamage(int damageToInflict)
    {
        _unitHealth -= damageToInflict;
        UpdateHealthUI();
    }
}
