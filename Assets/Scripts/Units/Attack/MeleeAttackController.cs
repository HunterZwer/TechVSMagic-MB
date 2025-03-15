using UnityEngine;

public class MeleeAttackController : MonoBehaviour
{
    public Transform targetToAttack;
    public bool isPlayer;
    public int unitDamage;
    private string _targetTag;

    [Header("Combat Settings")]
    public float attackRange = 1.2f;
    public float attackCooldown = 1.0f; // Cooldown time between attacks
    private float lastAttackTime;

    void Start()
    {
        SetTargetTag();
    }

    void Update()
    {
        if (targetToAttack != null && IsTargetDead())
        {
            targetToAttack = null;
        }

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
        _targetTag = isPlayer ? "Enemy" : "Player";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_targetTag is null){return;}
        if (other.CompareTag(_targetTag) && targetToAttack == null)
        {
            Unit unit = other.GetComponent<Unit>();
            if (unit != null && unit.GetCurrentHealth() > 0)
            {
                targetToAttack = other.transform;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(_targetTag) && targetToAttack == other.transform)
        {
            targetToAttack = null;
        }
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
        if (GetComponent<Unit>().GetCurrentHealth() <= 0) return; 

        if (targetToAttack != null && !IsTargetDead())
        {
            Unit targetUnit = targetToAttack.GetComponent<Unit>();
            if (targetUnit != null)
            {
                targetUnit.TakeDamage(unitDamage);
            }
        }
    }
}
