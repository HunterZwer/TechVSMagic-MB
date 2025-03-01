using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    private Transform target;
    private int damage;
    private float speed;
    private bool isPlayerProjectile;

    public void Initialize(Transform target, int damage, float speed)
    {
        this.target = target;
        this.damage = damage;
        this.speed = speed;
        this.isPlayerProjectile = isPlayerProjectile;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        transform.LookAt(target);
    }

    void OnTriggerEnter(Collider other)
    {
        string targetTag = isPlayerProjectile ? "Enemy" : "Player";
        
        if (other.CompareTag(targetTag))
        {
            float distance = Vector3.Distance(
                transform.position,
                other.transform.position
            );
            Unit targetUnit = other.GetComponent<Unit>();

            // Only trigger if within acceptable range
            if (distance <= 0.5f)
            {
                if (targetUnit != null)
                {
                    targetUnit.TakeDamage(damage);
                    Destroy(gameObject);
                }
            }
        }
    }
}


