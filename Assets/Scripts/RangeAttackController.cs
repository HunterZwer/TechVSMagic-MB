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
        lastAttackTime = -attackCooldown; // Allow immediate attack
    }

    void Update()
    {
        FindNearestTarget();
        
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
                float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = hitCollider.transform;
                }
            }
        }

        targetToAttack = closestTarget;
    }

    // Change this method to public
    public void Attack()
    {
        if (projectilePrefab && projectileSpawnPoint)
        {
            GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
            Projectile projectileScript = projectile.GetComponent<Projectile>();
            
            if (projectileScript == null)
            {
                projectileScript = projectile.AddComponent<Projectile>();
            }

            projectileScript.Initialize(targetToAttack, projectileDamage, projectileSpeed);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}