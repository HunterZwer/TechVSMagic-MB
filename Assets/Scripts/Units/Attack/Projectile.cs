using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform target;
    private int damage;
    private float speed;
    private bool isPlayerProjectile;

    public void Initialize(Transform target, int damage, float speed, bool isPlayer)
    {
        this.target = target;
        this.damage = damage;
        this.speed = speed;
        this.isPlayerProjectile = isPlayer;
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (target == null || target.GetComponent<Unit>()?.GetCurrentHealth() <= 0)
        {
            DestroyProjectile();
            return;
        }

        transform.LookAt(target);
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance < 0.5f)
        {
            HitTarget();
        }
    }

    void HitTarget()
    {
        if (target != null)
        {
            Unit targetUnit = target.GetComponent<Unit>();
            if (targetUnit != null)
            {
                targetUnit.TakeDamage(damage);
            }
        }
        DestroyProjectile();
    }

    void DestroyProjectile()
    {
        ObjectPool.Instance.ReturnObjectToPool(gameObject);
    }
}