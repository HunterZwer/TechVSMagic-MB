using UnityEngine;

public class RangeAttackController : AttackController
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    
    [Header("Combat Settings")]
    public float attackRange = 10f;
    public float attackCooldown = 1f;
    public float projectileSpeed = 20f;
    [SerializeField] private LayerMask _enemyLayerMask;

    private float _lastAttackTime;
    private float _attackRangeSq;
    private Collider[] _hitCollidersCache = new Collider[100]; // Cache collider array to avoid allocations
    
    // Upgrade levels - consider using a struct if these are always used together
    private int _rangeUpgradeLevel = 0;
    private int _reloadUpgradeLevel = 0;
    private int _damageUpgradeLevel = 0;

    protected void Start()
    {
        
        // Cache calculations
        attackRange *= unitStats.RangeMultiplier[_rangeUpgradeLevel];
        attackCooldown *= unitStats.ReloadMultiplier[_reloadUpgradeLevel];
        unitDamage *= unitStats.DamageMultiplier[_damageUpgradeLevel];
        _attackRangeSq = attackRange * attackRange;
        _lastAttackTime = Time.time - attackCooldown; // Allow immediate first attack
        
        FindNearestTarget();
    }

    void FixedUpdate()
    {
        if (ThisUnit.IsDead) return;
        
        // Check target validity
        if (IsTargetDead(targetToAttack))
        {
            FindNearestTarget();
            return;
        }

        if (targetToAttack == null) return;

        // Check attack cooldown and range
        if (Time.time - _lastAttackTime >= attackCooldown && 
            (targetToAttack.position - transform.position).sqrMagnitude <= _attackRangeSq)
        {
            Attack();
            _lastAttackTime = Time.time;
        }
    }

    void FindNearestTarget()
    {
        if (string.IsNullOrEmpty(targetTag)) return;
        
        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position, 
            attackRange, 
            _hitCollidersCache, 
            _enemyLayerMask
        );

        Transform closestTarget = null;
        float closestDistanceSq = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            var hitCollider = _hitCollidersCache[i];
            if (!hitCollider.CompareTag(targetTag)) continue;
            
            if (hitCollider.TryGetComponent(out Unit unit) && !unit.IsDead)
            {
                float distanceSq = (hitCollider.transform.position - transform.position).sqrMagnitude;
                if (distanceSq < closestDistanceSq)
                {
                    closestDistanceSq = distanceSq;
                    closestTarget = hitCollider.transform;
                }
            }
        }

        targetToAttack = closestTarget;
    }
    
    public void Attack()
    {
        if (projectilePrefab == null || projectileSpawnPoint == null || targetToAttack == null) 
            return;

        // Get or create projectile
        GameObject projectile = ObjectPool.Instance.GetPooledObject(projectilePrefab.name) ?? 
                              Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);

        // Get or add projectile component
        if (!projectile.TryGetComponent(out Projectile projectileScript))
        {
            projectileScript = projectile.AddComponent<Projectile>();
        }

        // Initialize and activate
        projectileScript.Initialize(targetToAttack, unitDamage, projectileSpeed, ThisUnit.IsPlayer);
        projectile.SetActive(true);
    }
}
