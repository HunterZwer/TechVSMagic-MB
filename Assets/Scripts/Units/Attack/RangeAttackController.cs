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
    private UnitLVL2 _cachedUnitLvl2; // Cache unitLvl2 component

    private float closestDistanceSq;
    private Transform closestTarget;
    private int hitCount;
    // Upgrade levels
    private int _rangeUpgradeLevel = 0;
    private int _reloadUpgradeLevel = 0;
    private int _damageUpgradeLevel = 0;

    protected void Start()
    {
        _cachedTransform = transform; // Cache transform
        _cachedUnitLvl2 = GetComponent<UnitLVL2>(); // Cache unitLvl2 reference

        attackRange *= unitStats.RangeMultiplier[_rangeUpgradeLevel];
        attackCooldown *= unitStats.ReloadMultiplier[_reloadUpgradeLevel];
        if (Upgrader.Instance is not null)
        {
            _damageUpgradeLevel = Upgrader.Instance.rangedDamageUpgradeLevel;
        }
        unitDamage *= unitStats.DamageMultiplier[_damageUpgradeLevel];
        _attackRangeSq = attackRange * attackRange;
        _lastAttackTime = Time.time - attackCooldown;
        
        InvokeRepeating(nameof(FindNearestTarget), Random.Range(0f, 0.3f), 0.5f);
    }
    
    public virtual void ApplyRangedDamageUpgrade()
    {
        unitDamage = baseDamage * unitStats.DamageMultiplier[Upgrader.Instance.rangedDamageUpgradeLevel];
    }
    public virtual void ApplyRangedUpgrade()
    {
        attackRange = baseRange * unitStats.DamageMultiplier[Upgrader.Instance.rangeUpgradeLevel];
        _attackRangeSq = attackRange * attackRange;
    }

    private void FindNearestTarget()
    {
        if (_cachedUnitLvl2.IsDead) return; // Use cached unitLvl2 reference

        hitCount = Physics.OverlapSphereNonAlloc(
            _cachedTransform.position, attackRange, _hitCollidersCache, _enemyLayerMask
        );

        closestTarget = null;
        closestDistanceSq = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCollider = _hitCollidersCache[i];
            hitCollider.TryGetComponent(out UnitLVL2 unit); // Directly get component reference

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

        projectileScript.Initialize(targetToAttack, unitDamage, projectileSpeed, _cachedUnitLvl2.IsPlayer);
        projectile.SetActive(true);
    }
}
