using UnityEngine;

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
    private Unit _unit;
    private float attackRangeSq;
    [SerializeField] public bool isPlayer;

    void Start()
    {
        SetTargetTag();
        _unit = GetComponent<Unit>();
        attackRangeSq = attackRange * attackRange;
        lastAttackTime = Time.time - attackCooldown;
        FindNearestTarget();
    }

    void Update()
    {
        if (targetToAttack == null || IsTargetDead())
        {
            FindNearestTarget();
        }

        if (targetToAttack != null && Time.time - lastAttackTime >= attackCooldown)
        {
            Vector3 direction = targetToAttack.position - transform.position;
            if (direction.sqrMagnitude <= attackRangeSq)
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
        float closestDistanceSq = Mathf.Infinity;
        Transform closestTarget = null;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(targetTag))
            {
                Unit unit = hitCollider.GetComponent<Unit>();
                if (unit != null && unit.GetCurrentHealth() > 0)
                {
                    Vector3 direction = hitCollider.transform.position - transform.position;
                    float distanceSq = direction.sqrMagnitude;
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
        if (_unit.GetCurrentHealth() <= 0) return;

        if (projectilePrefab && projectileSpawnPoint && targetToAttack != null)
        {
            GameObject projectile = ObjectPool.Instance.GetPooledObject(projectilePrefab.name);
            if (projectile == null)
            {
                projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
            }

            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript == null)
            {
                projectileScript = projectile.AddComponent<Projectile>();
            }

            projectileScript.Initialize(targetToAttack, projectileDamage, projectileSpeed, isPlayer);
            projectile.SetActive(true);
        }
    }
}