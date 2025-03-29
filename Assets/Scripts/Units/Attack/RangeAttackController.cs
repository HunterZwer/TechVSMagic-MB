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
    private readonly Collider[] _hitCollidersCache = new Collider[50]; // Reduce cache size based on expected enemies
    private Transform _cachedTransform; // Cache self transform
    private Unit _cachedUnit; // Cache unit component

    // Upgrade levels
    private int _rangeUpgradeLevel = 0;
    private int _reloadUpgradeLevel = 0;
    private int _damageUpgradeLevel = 0;

    protected void Start()
    {
        _cachedTransform = transform; // Cache transform
        _cachedUnit = GetComponent<Unit>(); // Cache unit reference

        attackRange *= unitStats.RangeMultiplier[_rangeUpgradeLevel];
        attackCooldown *= unitStats.ReloadMultiplier[_reloadUpgradeLevel];
        unitDamage *= unitStats.DamageMultiplier[_damageUpgradeLevel];
        _attackRangeSq = attackRange * attackRange;
        _lastAttackTime = Time.time - attackCooldown;

        // Call FindNearestTarget every 0.3 seconds
        InvokeRepeating(nameof(FindNearestTarget), 0f, 0.3f);
    }

    private void FindNearestTarget()
    {
        if (_cachedUnit.IsDead) return; // Use cached unit reference

        int hitCount = Physics.OverlapSphereNonAlloc(
            _cachedTransform.position, attackRange, _hitCollidersCache, _enemyLayerMask
        );

        Transform closestTarget = null;
        float closestDistanceSq = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCollider = _hitCollidersCache[i];
            Unit unit = hitCollider.GetComponent<Unit>(); // Directly get component reference

            if (unit == null || unit.IsDead) continue;

            Vector3 targetPos = hitCollider.transform.position; // Avoid multiple transform calls
            float distanceSq = (targetPos - _cachedTransform.position).sqrMagnitude;
            if (distanceSq < closestDistanceSq)
            {
                closestDistanceSq = distanceSq;
                closestTarget = hitCollider.transform;
            }
        }

        targetToAttack = closestTarget;
        TryAttack();
    }

    private void TryAttack()
    {
        if (targetToAttack == null || Time.time - _lastAttackTime < attackCooldown)
            return;

        Vector3 targetPos = targetToAttack.position; // Avoid multiple transform calls
        float distanceSq = (_cachedTransform.position - targetPos).sqrMagnitude;
        if (distanceSq > _attackRangeSq) return;

        Attack();
        _lastAttackTime = Time.time;
    }

    public void Attack()
    {
        if (projectilePrefab == null || projectileSpawnPoint == null || targetToAttack == null) 
            return;

        GameObject projectile = ObjectPool.Instance.GetPooledObject(projectilePrefab.name) ?? 
                                Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);

        Projectile projectileScript = projectile.GetComponent<Projectile>(); // Direct get instead of TryGetComponent
        if (projectileScript == null)
        {
            projectileScript = projectile.AddComponent<Projectile>();
        }

        projectileScript.Initialize(targetToAttack, unitDamage, projectileSpeed, _cachedUnit.IsPlayer);
        projectile.SetActive(true);
    }
}
