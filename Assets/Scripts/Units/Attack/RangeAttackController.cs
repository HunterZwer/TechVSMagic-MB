using UnityEngine;
using System.Collections;

public class RangeAttackController : MonoBehaviour
{
    public Transform targetToAttack;
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;

    [Header("Combat Settings")]
    public float attackRange = 10f;
    public float attackCooldown = 1f;
    public int projectileDamage = 10;
    public float projectileSpeed = 20f;

    private float lastAttackTime;
    private string targetTag;
    [SerializeField]
    float maxHitDistance = 1.5f;
    public bool isPlayer;

    void Start()
    {
        SetTargetTag();
        lastAttackTime = -attackCooldown;
    }

    void Update()
    {
        if (targetToAttack == null || IsTargetDead())
        {
            FindNearestTarget();
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

    void SetTargetTag()
    {
        targetTag = isPlayer ? "Enemy" : "Player";
    }

    void FindNearestTarget()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
        float closestDistance = Mathf.Infinity;
        Transform closestTarget = null;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(targetTag))
            {
                Unit unit = hitCollider.GetComponent<Unit>();
                if (unit != null && unit.GetCurrentHealth() > 0)
                {
                    float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = hitCollider.transform;
                    }
                }
            }
        }

        targetToAttack = closestTarget;
    }

    bool IsTargetDead()
    {
        if (targetToAttack != null)
        {
            Unit unit = targetToAttack.GetComponent<Unit>();
            return unit == null || unit.GetCurrentHealth() <= 0;
        }
        return true;
    }

    public void Attack()
    {
        if (GetComponent<Unit>().GetCurrentHealth() <= 0) return; // Юнит мертв - не атакует

        if (projectilePrefab && projectileSpawnPoint && targetToAttack != null && !IsTargetDead())
        {
            GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
            Projectile projectileScript = projectile.GetComponent<Projectile>();

            if (projectileScript == null)
            {
                projectileScript = projectile.AddComponent<Projectile>();
            }

            projectileScript.Initialize(targetToAttack, projectileDamage, projectileSpeed, isPlayer);
        }
    }
}