using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform target;
    private int damage;
    private float speed;
    private bool isPlayerProjectile;

    public void Initialize(Transform target, int damage, float speed, bool isPlayerProjectile)
    {
        this.target = target;
        this.damage = damage;
        this.speed = speed;
        this.isPlayerProjectile = isPlayerProjectile; // Set the isPlayerProjectile flag
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Move towards the target
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        transform.LookAt(target);
    }

    void OnTriggerEnter(Collider other)
    {
        // Determine the target tag based on the projectile type
        string targetTag = isPlayerProjectile ? "Enemy" : "Player";

        // Check if the collided object has the correct tag
        if (other.CompareTag(targetTag))
        {
            // Apply damage to the target
            Unit targetUnit = other.GetComponent<Unit>();
            if (targetUnit != null)
            {
                targetUnit.TakeDamage(damage);
            }

            // Destroy the projectile
            Destroy(gameObject);
        }
    }
}