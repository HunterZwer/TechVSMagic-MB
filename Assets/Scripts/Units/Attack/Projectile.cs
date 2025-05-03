using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform target;
    private float damage;
    private float speed;
    private Coroutine autoDestroyCoroutine;
    private Vector3 direction;

    public void Initialize(Transform target, float damage, float speed, bool isPlayer)
    {
        this.target = target;
        this.damage = damage;
        this.speed = speed;

        if (target != null)
            direction = (target.position - transform.position).normalized;

        gameObject.SetActive(true);

        if (autoDestroyCoroutine != null)
            StopCoroutine(autoDestroyCoroutine);
        autoDestroyCoroutine = StartCoroutine(AutoDestroyAfterTime(3f));
    }

    void FixedUpdate()
    {
        if (!target || (target.TryGetComponent(out UnitLVL2 unit) && unit.IsDead))
        {
            DestroyProjectile();
            return;
        }


        transform.LookAt(target);
        transform.Translate(direction * (speed * Time.fixedDeltaTime), Space.World);

        
        float distanceSqr = (transform.position - target.position).sqrMagnitude;
        if (distanceSqr < 0.25f) // 0.5 * 0.5
        {
            HitTarget();
        }

    }

    void HitTarget()
    {
        if (target && target.TryGetComponent(out UnitLVL2 targetUnit))
        {
            targetUnit.TakeDamage(damage);
        }
        DestroyProjectile();
    }

    void DestroyProjectile()
    {
        ObjectPool.Instance.ReturnObjectToPool(gameObject);
    }
    
    IEnumerator AutoDestroyAfterTime(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        DestroyProjectile();
    }
    
    void OnDisable()
    {
        if (autoDestroyCoroutine != null)
            StopCoroutine(autoDestroyCoroutine);
    }

}