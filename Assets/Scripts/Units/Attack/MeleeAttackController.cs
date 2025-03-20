using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeleeAttackController : MonoBehaviour
{
    public Transform targetToAttack;
    public float unitDamage;
    private string _targetTag;

    [Header("Combat Settings")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1.0f;
    private bool isAttacking;
    private float attackRangeSquared;
    private Unit _unit;
    private UnitStats unitStats;
    
    [Header("Upgrade Levels")]
    private int _rangeUprgadeLevel = 0;
    private int _reloadUprgadeLevel = 0;
    private int _damageUprgadeLevel = 0;

    private List<EnemyInfo> enemiesInRange = new List<EnemyInfo>();

    private void Start()
    {
        _unit = GetComponent<Unit>();
        SetTargetTag();
        attackRangeSquared = attackRange * attackRange;
        StartCoroutine(AttackLoop());
        
        unitStats = JsonLoader.LoadUnitStats(_unit.unitClass, _unit.IsPlayer);

        // Override combat settings with JSON data
        attackRange = attackRange * unitStats.RangeMultiplier[_rangeUprgadeLevel];
        attackCooldown = attackCooldown * unitStats.ReloadMultiplier[_reloadUprgadeLevel];
        unitDamage = unitDamage * unitStats.DamageMultiplier[_damageUprgadeLevel];
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (_targetTag == null) return;
        if (other.CompareTag(_targetTag))
        {
            Unit unit = other.GetComponent<Unit>();
            if (unit != null && !unit.IsDead)
            {
                enemiesInRange.Add(new EnemyInfo(other.transform, unit));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag is null) return;
        if (other.CompareTag(_targetTag))
        {
            enemiesInRange.RemoveAll(ei => ei.Transform == other.transform);
            
            if (targetToAttack != null && targetToAttack == other.transform)
            {
                targetToAttack = GetClosestEnemy();
            }
        }
    }

    private Transform GetClosestEnemy()
    {
        Transform closestEnemy = null;
        float closestDistanceSq = Mathf.Infinity;

        foreach (EnemyInfo enemy in enemiesInRange)
        {
            if (enemy.Unit is null || enemy.Unit.IsDead) continue;

            Vector3 direction = enemy.Transform.position - transform.position;
            float distanceSq = direction.sqrMagnitude;
            if (distanceSq < closestDistanceSq)
            {
                closestDistanceSq = distanceSq;
                closestEnemy = enemy.Transform;
            }
        }

        return closestEnemy;
    }

    private void SetTargetTag()
    {
        _targetTag = _unit.IsPlayer ? "Enemy" : "Player";
    }

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
            
            CleanupDeadTargets();
            targetToAttack = GetClosestEnemy();

            if (targetToAttack != null && !_unit.IsDead && !isAttacking)
            {
                Vector3 direction = targetToAttack.position - transform.position;
                if (direction.sqrMagnitude <= attackRangeSquared)
                {
                    StartCoroutine(PerformAttack());
                }
            }
        }
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        yield return new WaitForEndOfFrame();
        Attack();
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    private void CleanupDeadTargets()
    {
        enemiesInRange.RemoveAll(ei => ei.Unit is null || ei.Unit.GetCurrentHealth() <= 0);
    }

    public void Attack()
    {
        if (_unit.IsDead) return;

        if (targetToAttack)
        {
            Unit targetUnit = targetToAttack.GetComponent<Unit>();
            if (targetUnit && !targetUnit.IsDead)
            {
                targetUnit.TakeDamage(unitDamage);
            }
        }
    }

    private struct EnemyInfo
    {
        public Transform Transform;
        public Unit Unit;

        public EnemyInfo(Transform transform, Unit unit)
        {
            Transform = transform;
            Unit = unit;
        }
    }
    
}