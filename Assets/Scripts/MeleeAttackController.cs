using UnityEngine;

public class MeleeAttackController : MonoBehaviour
{
    public Transform targetToAttack;
    public bool isPlayer;
    public int unitDamage;
    private string _targetTag;

    void Start()
    {
        SetTargetTag();
        Debug.Log("Target Tag: " + _targetTag);
    }

    void SetTargetTag()
    {
        _targetTag = isPlayer ? "Enemy" : "Player";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_targetTag) && targetToAttack == null)
        {
            targetToAttack = other.transform;
            Debug.Log("Target found: " + other.name);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(_targetTag) && targetToAttack == null)
        {
            targetToAttack = other.transform;
            Debug.Log("Target in range: " + other.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(_targetTag) && targetToAttack != null)
        {
            targetToAttack = null;
            Debug.Log("Target lost: " + other.name);
        }
    }

    // Add this method to handle melee attacks
    public void Attack()
    {
        if (targetToAttack != null)
        {
            // Assuming the target has a component called "Unit" with a "TakeDamage" method
            Unit targetUnit = targetToAttack.GetComponent<Unit>();
            if (targetUnit != null)
            {
                targetUnit.TakeDamage(unitDamage);
                Debug.Log("Attacked: " + targetToAttack.name + " for " + unitDamage + " damage");
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 10f * 0.2f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 1.2f);
    }
}