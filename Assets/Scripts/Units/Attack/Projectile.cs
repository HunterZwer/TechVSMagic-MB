using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform target;
    private float damage;
    private float speed;
    private bool isPlayerProjectile;

    public void Initialize(Transform target, float damage, float speed, bool isPlayer)
    {
        this.target = target;
        this.damage = damage;
        this.speed = speed;
        this.isPlayerProjectile = isPlayer;
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!target || (target.TryGetComponent(out Unit unit) && unit.IsDead))
        {
            DestroyProjectile();
            return;
        }


        transform.LookAt(target);
        transform.Translate(Vector3.forward * (speed * Time.deltaTime));

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance < 0.5f)
        {
            HitTarget();
        }
    }

    void HitTarget()
    {
        if (target && target.TryGetComponent(out Unit targetUnit))
        {
            targetUnit.TakeDamage(damage);
        }
        DestroyProjectile();
    }

    void DestroyProjectile()
    {
        ObjectPool.Instance.ReturnObjectToPool(gameObject);
    }
}