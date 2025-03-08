using UnityEngine;

public class MeleeAttackController : MonoBehaviour
{
    public Transform targetToAttack;
    public bool isPlayer;
    public int unitDamage;
    private string _targetTag;
    [Header("Combat Settings")]
    public float attackRange = 1.2f;
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
    }

    void SetTargetTag()
    {
        _targetTag = isPlayer ? "Enemy" : "Player";
    }

    private void OnTriggerEnter(Collider other)
    {
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
