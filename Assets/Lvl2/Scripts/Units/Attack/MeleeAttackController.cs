using UnityEngine;
using System.Collections.Generic;

public class MeleeAttackController : AttackController
{
    [Header("Combat Settings")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1.0f;

    [SerializeField] private LayerMask _enemyLayerMask;
    
    private bool isAttacking;
    private float attackRangeSquared;
    private int _rangeUpgradeLevel = 0;
    private int _reloadUpgradeLevel = 0;
    private int _damageUpgradeLevel = 0;
    private readonly Dictionary<Transform, UnitLVL2> enemiesInRange = new();
    private Transform closestEnemy;
    private float closestDistanceSq = float.MaxValue;
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    private Animator _animator;
    
    private void Start()
    {
        attackRangeSquared = attackRange * attackRange;

        // Apply upgrades
        attackRange *= unitStats.RangeMultiplier[_rangeUpgradeLevel];
        attackCooldown *= unitStats.ReloadMultiplier[_reloadUpgradeLevel];
        if (Upgrader.Instance is not null){_damageUpgradeLevel = Upgrader.Instance.damageUpgradeLevel;}
        unitDamage *= unitStats.DamageMultiplier[_damageUpgradeLevel];

        // Start periodic enemy checks
        InvokeRepeating(nameof(PeriodicUpdate), Random.Range(0f, 0.25f), 0.5f);
        _animator = GetComponent<Animator>();
    }
    
    public virtual void ApplyDamageUpgrade()
    {
        unitDamage = baseDamage * unitStats.DamageMultiplier[Upgrader.Instance.damageUpgradeLevel];
    }
    
    private void PeriodicUpdate()
    {
        if (thisUnitLvl2.IsDead || enemiesInRange.Count == 0) return;
        CleanupDeadTargets();
        UpdateTarget();  // Only update if needed
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsValidTarget(other)) return;

        if (other.TryGetComponent(out UnitLVL2 unit) && !unit.IsDead)
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
        return (_enemyLayerMask.value & (1 << other.gameObject.layer)) != 0;
    }

    private void UpdateTarget()
    {
        if (thisUnitLvl2.IsDead) return;

        CleanupDeadTargets();
    
        // Update closest enemy only if necessary
        if (targetToAttack == null || targetToAttack.GetComponent<UnitLVL2>().IsDead)
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
            UnitLVL2 enemyUnitLvl2 = kvp.Value;

            if (enemyUnitLvl2.IsDead) continue;

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
        if (_animator.GetBool(IsMoving)) return;
        if (isAttacking || thisUnitLvl2.IsDead || targetToAttack == null) return;

        if (targetToAttack.TryGetComponent(out UnitLVL2 targetUnit) && targetUnit != null && !targetUnit.IsDead)
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
        if (thisUnitLvl2.IsDead || targetToAttack == null) return;
        
        if (targetToAttack.TryGetComponent(out UnitLVL2 targetUnit) && targetUnit != null && !targetUnit.IsDead)
        {
            targetUnit.TakeDamage(unitDamage);
        }        
    }
}
