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
    private Collider[] _hitCollidersCache = new Collider[50]; // Reduce cache size based on expected enemies
    private Transform _cachedTransform; // Cache self transform

    // Upgrade levels
    private int _rangeUpgradeLevel = 0;
    private int _reloadUpgradeLevel = 0;
    private int _damageUpgradeLevel = 0;

    protected void Start()
    {
        _cachedTransform = transform; // Cache transform
        attackRange *= unitStats.RangeMultiplier[_rangeUpgradeLevel];
        attackCooldown *= unitStats.ReloadMultiplier[_reloadUpgradeLevel];
        unitDamage *= unitStats.DamageMultiplier[_damageUpgradeLevel];
        _attackRangeSq = attackRange * attackRange;
        _lastAttackTime = Time.time - attackCooldown;

        FindNearestTarget();
    }

    void FixedUpdate()
    {
        if (ThisUnit.IsDead) return;

        if (targetToAttack == null || IsTargetDead(targetToAttack))
        {
            FindNearestTarget();
            return;
        }

        float distanceSq = (_cachedTransform.position - targetToAttack.position).sqrMagnitude;
        if (Time.time - _lastAttackTime >= attackCooldown && distanceSq <= _attackRangeSq)
        {
            Attack();
            _lastAttackTime = Time.time;
        }
    }

    void FindNearestTarget()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(
            _cachedTransform.position, attackRange, _hitCollidersCache, _enemyLayerMask
        );

        Transform closestTarget = null;
        float closestDistanceSq = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCollider = _hitCollidersCache[i];

            // Avoid CompareTag inside loops - use LayerMask filtering instead
            if (!hitCollider.TryGetComponent(out Unit unit) || unit.IsDead) continue;

            float distanceSq = (hitCollider.transform.position - _cachedTransform.position).sqrMagnitude;
            if (distanceSq < closestDistanceSq)
            {
                closestDistanceSq = distanceSq;
                closestTarget = hitCollider.transform;
            }
        }

        targetToAttack = closestTarget;
    }

    public void Attack()
    {
        if (projectilePrefab == null || projectileSpawnPoint == null || targetToAttack == null) 
            return;

        GameObject projectile = ObjectPool.Instance.GetPooledObject(projectilePrefab.name) ?? 
                                Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);

        if (!projectile.TryGetComponent(out Projectile projectileScript))
            projectileScript = projectile.AddComponent<Projectile>();

        projectileScript.Initialize(targetToAttack, unitDamage, projectileSpeed, ThisUnit.IsPlayer);
        projectile.SetActive(true);
    }
}
