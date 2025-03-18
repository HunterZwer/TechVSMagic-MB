using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeleeAttackController : MonoBehaviour
{
    public Transform targetToAttack;
    public bool isPlayer;
    public int unitDamage;
    private string _targetTag;

    [Header("Combat Settings")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1.0f;
    private bool isAttacking;
    private float attackRangeSquared;
    private Unit _unit;
    private UnitStats unitStats;

    private List<EnemyInfo> enemiesInRange = new List<EnemyInfo>();

    private void Start()
    {
        SetTargetTag();
        _unit = GetComponent<Unit>();
        attackRangeSquared = attackRange * attackRange;
        StartCoroutine(AttackLoop());
        
        LoadUnitStats();

        // Override combat settings with JSON data
        attackRange = unitStats.range;
        attackCooldown = unitStats.reload;
        unitDamage = unitStats.damage;
    }
    
    private void LoadUnitStats()
    {
        // Use the JsonLoader to load the unit stats based on the unit's class and team
        unitStats = JsonLoader.LoadUnitStats(_unit.unitClass, isPlayer);

        if (unitStats == null)
        {
            Debug.LogError("Failed to load unit stats. Using default values.");
            unitStats = new UnitStats
            {
                health = 100,
                damage = 10,
                range = 1.5f,
                reload = 1.0f
            };
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_targetTag == null) return;
        if (other.CompareTag(_targetTag))
        {
            Unit unit = other.GetComponent<Unit>();
            if (unit != null && unit.GetCurrentHealth() > 0)
            {
                enemiesInRange.Add(new EnemyInfo(other.transform, unit));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
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
            if (enemy.Unit == null || enemy.Unit.GetCurrentHealth() <= 0) continue;

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
        _targetTag = isPlayer ? "Enemy" : "Player";
    }

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
            
            CleanupDeadTargets();
            targetToAttack = GetClosestEnemy();

            if (targetToAttack != null && !IsTargetDead(_unit.transform) && !isAttacking)
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

        if (!IsTargetDead(_unit.transform) && targetToAttack != null)
        {
            Unit targetUnit = targetToAttack.GetComponent<Unit>();
            if (targetUnit != null && targetUnit.GetCurrentHealth() > 0)
            {
                targetUnit.TakeDamage(unitDamage);
            }
        }

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    private void CleanupDeadTargets()
    {
        enemiesInRange.RemoveAll(ei => ei.Unit == null || ei.Unit.GetCurrentHealth() <= 0);
    }

    public void Attack()
    {
        if (IsTargetDead(_unit.transform)) return;

        if (targetToAttack != null)
        {
            Unit targetUnit = targetToAttack.GetComponent<Unit>();
            if (targetUnit != null && targetUnit.GetCurrentHealth() > 0)
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
    
    private bool IsTargetDead(Transform target)
    {
        if (target == null) return true;
        Unit unit = target.GetComponent<Unit>();
        return unit == null || unit.GetCurrentHealth() <= 0;
    }
}