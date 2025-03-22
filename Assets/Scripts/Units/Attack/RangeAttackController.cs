using UnityEngine;

public class RangeAttackController : AttackController
{
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;

    [Header("Combat Settings")]
    public float attackRange = 10f;
    public float attackCooldown = 1f;
    public int projectileDamage = 10;
    public float projectileSpeed = 20f;

    private float lastAttackTime;
    private float attackRangeSq;

    void Start()
    {
        targetTag = SetTargetTag(_unit);
        attackRangeSq = attackRange * attackRange;
        lastAttackTime = Time.time - attackCooldown;
        FindNearestTarget();
    }

    void Update()
    {
        // Check if the target is null or dead and find a new target if necessary
        if (IsTargetDead(targetToAttack))
        {
            FindNearestTarget();
        }

        // If there is no valid target, return early
        if (targetToAttack is null)
        {
            return;
        }

        // Check if it's time to attack
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            Vector3 direction = targetToAttack.position - transform.position;
            if (direction.sqrMagnitude <= attackRangeSq)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
    }
    

    void FindNearestTarget()
    {
        if (string.IsNullOrEmpty(targetTag)) return;
        Collider[] hitColliders = new Collider[100];
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, attackRange, hitColliders);
        float closestDistanceSq = Mathf.Infinity;
        Transform closestTarget = null;
        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCollider = hitColliders[i];
            if (hitCollider.CompareTag(targetTag))
            {
                hitCollider.TryGetComponent(out Unit unit);
                if (unit && !unit.IsDead)
                {
                    float distanceSq = (hitCollider.transform.position - transform.position).sqrMagnitude;

                    if (distanceSq < closestDistanceSq)
                    {
                        closestDistanceSq = distanceSq;
                        closestTarget = hitCollider.transform;
                    }
                }
            }
        }

        targetToAttack = closestTarget;
    }




    public void Attack()
    {
        
        if (_unit.IsDead)
        {
            return;
        }

        if (projectilePrefab && projectileSpawnPoint && targetToAttack)
        {
            GameObject projectile = ObjectPool.Instance.GetPooledObject(projectilePrefab.name);
            if (!projectile)
            {
                projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
            }

            projectile.TryGetComponent(out Projectile projectileScript);
            if (!projectileScript)
            {
                projectileScript = projectile.AddComponent<Projectile>();
            }

            projectileScript.Initialize(targetToAttack, projectileDamage, projectileSpeed, _unit.IsPlayer);
            projectile.SetActive(true);
        }
    }
}