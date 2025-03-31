using UnityEngine;
using System.Collections.Generic;

public class MeleeAttackController : AttackController
{
    [Header("Combat Settings")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1.0f;

    private bool isAttacking;
    private float attackRangeSquared;
    private int _rangeUpgradeLevel = 0;
    private int _reloadUpgradeLevel = 0;
    private int _damageUpgradeLevel = 0;
    private readonly Dictionary<Transform, Unit> enemiesInRange = new();
    private Transform closestEnemy;
    private float closestDistanceSq = float.MaxValue;
    
    private void Start()
    {
        attackRangeSquared = attackRange * attackRange;

        // Apply upgrades
        attackRange *= unitStats.RangeMultiplier[_rangeUpgradeLevel];
        attackCooldown *= unitStats.ReloadMultiplier[_reloadUpgradeLevel];
        unitDamage *= unitStats.DamageMultiplier[_damageUpgradeLevel];

        // Start periodic enemy checks
        InvokeRepeating(nameof(PeriodicUpdate), Random.Range(0f, 0.25f), 0.5f);
    }
    
    private void PeriodicUpdate()
    {
        if (ThisUnit.IsDead) return;

        CleanupDeadTargets();
        UpdateTarget();  // Only update if needed
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsValidTarget(other)) return;

        if (other.TryGetComponent(out Unit unit) && !unit.IsDead)
        {
            enemiesInRange[other.transform] = unit;;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsValidTarget(other)) return;

        enemiesInRange.Remove(other.transform);

        if (targetToAttack == other.transform)
        {
            targetToAttack = GetClosestEnemy();
        }
    }

    private bool IsValidTarget(Collider other)
    {
        return !(string.IsNullOrEmpty(targetTag) || !other.CompareTag(targetTag));
    }

    private void UpdateTarget()
    {
        if (ThisUnit.IsDead) return;

        CleanupDeadTargets();
    
        // Update closest enemy only if necessary
        if (targetToAttack == null || targetToAttack.GetComponent<Unit>().IsDead)
        {
            targetToAttack = GetClosestEnemy();
        }

        if (targetToAttack != null && !isAttacking)
        {
            float distanceSq = (targetToAttack.position - transform.position).sqrMagnitude;
            if (distanceSq <= attackRangeSquared)
            {
                PerformAttack();
            }
        }
    }

    private Transform GetClosestEnemy()
    {
        Vector3 myPosition = transform.position;
        closestEnemy = null;
        closestDistanceSq = float.MaxValue;

        foreach (var kvp in enemiesInRange)
        {
            Transform enemyTransform = kvp.Key;
            Unit enemyUnit = kvp.Value;

            if (enemyUnit.IsDead) continue;

            float distanceSq = (enemyTransform.position - myPosition).sqrMagnitude;
            if (distanceSq < closestDistanceSq)
            {
                closestDistanceSq = distanceSq;
                closestEnemy = enemyTransform;
            }
        }

        return closestEnemy;
    }

    private void PerformAttack()
    {
        if (isAttacking || ThisUnit.IsDead || targetToAttack == null) return;

        if (targetToAttack.TryGetComponent(out Unit targetUnit) && targetUnit != null && !targetUnit.IsDead)
        {
            isAttacking = true;
            Attack();
            Invoke(nameof(ResetAttack), attackCooldown);
        }
    }

    private void ResetAttack()
    {
        isAttacking = false;
    }

    private void CleanupDeadTargets()
    {
        List<Transform> toRemove = new();

        foreach (var kvp in enemiesInRange)
        {
            if (kvp.Value == null || kvp.Value.IsDead)
            {
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var key in toRemove)
        {
            enemiesInRange.Remove(key);
        }
    }

    public void Attack()
    {
        if (ThisUnit.IsDead || targetToAttack == null) return;

        if (targetToAttack.TryGetComponent(out Unit targetUnit) && targetUnit != null && !targetUnit.IsDead)
        {
            targetUnit.TakeDamage(unitDamage);
        }
    }

    private readonly struct EnemyInfo
    {
        public readonly Transform Transform;
        public readonly Unit Unit;

        public EnemyInfo(Transform transform, Unit unit)
        {
            Transform = transform;
            Unit = unit;
        }
    }
}
