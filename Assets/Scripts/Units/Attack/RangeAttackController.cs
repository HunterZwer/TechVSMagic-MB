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
        // Check if the target is null or dead and find a new target if necessary
        if (targetToAttack == null || IsTargetDead())
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

    void SetTargetTag()
    {
        targetTag = isPlayer ? "Enemy" : "Player";
    }

    void FindNearestTarget()
    {
        Collider[] hitColliders = new Collider[100];
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, attackRange, hitColliders);
        float closestDistanceSq = Mathf.Infinity;
        Transform closestTarget = null;

        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCollider = hitColliders[i];

            if (hitCollider.CompareTag(targetTag))
            {
                Unit unit = hitCollider.GetComponent<Unit>();
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


    bool IsTargetDead()
    {
        if (targetToAttack != null)
        {
            Unit unit = targetToAttack.GetComponent<Unit>();
            return unit == null || !unit.IsDead;
        }
        return true;
    }

    public void Attack()
    {
        
        if (_unit.IsDead)
        {
            return;
        }

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