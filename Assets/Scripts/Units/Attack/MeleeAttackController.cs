using UnityEngine;
using System.Collections;
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
        StartCoroutine(AttackLoop());
        
        // Apply upgrades
        attackRange *= unitStats.RangeMultiplier[_rangeUpgradeLevel];
        attackCooldown *= unitStats.ReloadMultiplier[_reloadUpgradeLevel];
        unitDamage *= unitStats.DamageMultiplier[_damageUpgradeLevel];
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

    private IEnumerator AttackLoop()
    {
        WaitForSeconds waitInterval = new WaitForSeconds(0.2f);
        WaitForSeconds cooldownWait = new WaitForSeconds(attackCooldown);
        
        while (true)
        {
            yield return waitInterval;
            
            CleanupDeadTargets();
            targetToAttack = GetClosestEnemy();

            if (targetToAttack != null && !ThisUnit.IsDead && !isAttacking)
            {
                float distanceSq = (targetToAttack.position - transform.position).sqrMagnitude;
                if (distanceSq <= attackRangeSquared)
                {
                    StartCoroutine(PerformAttack(cooldownWait));
                }
            }
        }
    }

    private IEnumerator PerformAttack(WaitForSeconds cooldownWait)
    {
        isAttacking = true;
        yield return null; // WaitForEndOfFrame is less efficient than yield return null
        Attack();
        yield return cooldownWait;
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
