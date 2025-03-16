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
    public float attackRange = 1.5f;  // Slightly increased for reliability
    public float attackCooldown = 1.0f;
    private bool isAttacking = false;

    private List<Transform> enemiesInRange = new List<Transform>(); // Track multiple enemies

    private void Start()
    {
        SetTargetTag();
        StartCoroutine(AttackLoop()); // Coroutine for attacking
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_targetTag is null) return;
        if (other.CompareTag(_targetTag))
        {
            Unit unit = other.GetComponent<Unit>();
            if (unit != null && unit.GetCurrentHealth() > 0)
            {
                enemiesInRange.Add(other.transform);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(_targetTag))
        {
            enemiesInRange.Remove(other.transform);

            if (targetToAttack == other.transform)
            {
                targetToAttack = GetClosestEnemy();
            }
        }
    }

    private Transform GetClosestEnemy()
    {
        Transform closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (Transform enemy in enemiesInRange)
        {
            if (enemy == null || IsTargetDead(enemy)) continue;

            float distance = Vector3.Distance(transform.position, enemy.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }

    private bool IsTargetDead(Transform target)
    {
        if (target == null) return true;
        Unit unit = target.GetComponent<Unit>();
        return unit == null || unit.GetCurrentHealth() <= 0;
    }

    private void SetTargetTag()
    {
        _targetTag = isPlayer ? "Enemy" : "Player";
    }

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f); // Check frequently but not every frame
            
            CleanupDeadTargets(); // Remove dead enemies
            targetToAttack = GetClosestEnemy(); // Always get the closest target

            if (targetToAttack != null && !isAttacking)
            {
                float distance = Vector3.Distance(transform.position, targetToAttack.position);
                
                if (distance <= attackRange)
                {
                    StartCoroutine(PerformAttack());
                }
            }
        }
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;

        if (GetComponent<Unit>().GetCurrentHealth() > 0 && targetToAttack != null && !IsTargetDead(targetToAttack))
        {
            Unit targetUnit = targetToAttack.GetComponent<Unit>();
            if (targetUnit != null)
            {
                targetUnit.TakeDamage(unitDamage);
            }
        }

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    private void CleanupDeadTargets()
    {
        enemiesInRange.RemoveAll(enemy => enemy == null || IsTargetDead(enemy));
    }
    
    public void Attack()
    {
        if (GetComponent<Unit>().GetCurrentHealth() <= 0) return;

        if (targetToAttack != null && !IsTargetDead(targetToAttack))
        {
            Unit targetUnit = targetToAttack.GetComponent<Unit>();
            if (targetUnit != null)
            {
                targetUnit.TakeDamage(unitDamage);
            }
        }
    }
}
