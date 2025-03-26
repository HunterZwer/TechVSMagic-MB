using UnityEngine;

public class RangeAttackController : AttackController
{
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    [SerializeField] private LayerMask _enemyLayerMask;
    [Header("Combat Settings")]
    public float attackRange = 10f;
    public float attackCooldown = 1f;
    public float projectileSpeed = 20f;

    private float lastAttackTime;
    private float attackRangeSq;
    
    private int _rangeUprgadeLevel = 0;
    private int _reloadUprgadeLevel = 0;
    private int _damageUprgadeLevel = 0;
    

    void Start()
    {
        attackRange = attackRange * unitStats.RangeMultiplier[_rangeUprgadeLevel];
        attackCooldown = attackCooldown * unitStats.ReloadMultiplier[_reloadUprgadeLevel];
        unitDamage = unitDamage * unitStats.DamageMultiplier[_damageUprgadeLevel];
        attackRangeSq = attackRange * attackRange;
        lastAttackTime = Time.time - attackCooldown;
        FindNearestTarget();
    }

    void FixedUpdate()
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
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, attackRange, hitColliders, _enemyLayerMask);
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
        
        if (ThisUnit.IsDead)
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

            projectileScript.Initialize(targetToAttack, unitDamage, projectileSpeed, ThisUnit.IsPlayer);
            projectile.SetActive(true);
        }
    }
}