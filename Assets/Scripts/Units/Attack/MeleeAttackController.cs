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
    private readonly HashSet<EnemyInfo> enemiesInRange = new HashSet<EnemyInfo>();

    private void Start()
    {
        attackRangeSquared = attackRange * attackRange;

        // Apply upgrades
        attackRange *= unitStats.RangeMultiplier[_rangeUpgradeLevel];
        attackCooldown *= unitStats.ReloadMultiplier[_reloadUpgradeLevel];
        unitDamage *= unitStats.DamageMultiplier[_damageUpgradeLevel];

        // Start periodic enemy checks
        InvokeRepeating(nameof(UpdateTarget), 0f, 0.25f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsValidTarget(other)) return;

        if (other.TryGetComponent(out Unit unit) && !unit.IsDead)
        {
            enemiesInRange.Add(new EnemyInfo(other.transform, unit));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsValidTarget(other)) return;

        enemiesInRange.RemoveWhere(ei => ei.Transform == other.transform);

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
        targetToAttack = GetClosestEnemy();

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
        Transform closestEnemy = null;
        float closestDistanceSq = float.MaxValue;
        Vector3 myPosition = transform.position;

        foreach (EnemyInfo enemy in enemiesInRange)
        {
            if (enemy.Unit == null || enemy.Unit.IsDead) continue;

            float distanceSq = (enemy.Transform.position - myPosition).sqrMagnitude;
            if (distanceSq < closestDistanceSq)
            {
                closestDistanceSq = distanceSq;
                closestEnemy = enemy.Transform;
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
        enemiesInRange.RemoveWhere(ei => ei.Unit == null || ei.Unit.IsDead);
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
