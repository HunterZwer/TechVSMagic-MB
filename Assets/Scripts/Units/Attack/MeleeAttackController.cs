using UnityEngine;
using System.Collections.Generic;

public class MeleeAttackController : MonoBehaviour
{
    public Transform targetToAttack;
    public bool isPlayer;
    public int unitDamage;
    private string _targetTag;

    [Header("Combat Settings")]
    public float attackRange = 1.2f;
    public float attackCooldown = 1.0f;
    private float lastAttackTime;

    private List<Transform> enemiesInRange = new List<Transform>(); // Track multiple enemies

    private void Start()
    {
        SetTargetTag();
    }

    private void Update()
    {
        CleanupDeadTargets(); // Remove dead targets

        if (targetToAttack == null)
        {
            targetToAttack = GetClosestEnemy();
        }

        if (targetToAttack != null && Time.time - lastAttackTime >= attackCooldown)
        {
            if (Vector3.Distance(transform.position, targetToAttack.position) <= attackRange)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
    }

    private void SetTargetTag()
    {
        _targetTag = isPlayer ? "Enemy" : "Player";
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

    private void CleanupDeadTargets()
    {
        enemiesInRange.RemoveAll(enemy => enemy == null || IsTargetDead(enemy));
    }

    private Transform GetClosestEnemy()
    {
        Transform closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (Transform enemy in enemiesInRange)
        {
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
