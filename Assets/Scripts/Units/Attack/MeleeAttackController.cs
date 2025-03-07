using UnityEngine;

public class MeleeAttackController : MonoBehaviour
{
    public Transform targetToAttack;
    public bool isPlayer;
    public int unitDamage;
    
    [Header("Combat Settings")]
    public float attackRange = 1.2f;
    
    private string _targetTag;
    private float lastAttackTime;

    private void Awake()
    {
        SetTargetTag();
    }

    void SetTargetTag()
    {
        _targetTag = isPlayer ? "Enemy" : "Player";
        
        // Ensure tag exists
        try
        {
            // This will throw an error if tag doesn't exist
            gameObject.CompareTag(_targetTag); 
        }
        catch
        {
            Debug.LogError($"Tag '{_targetTag}' is not defined in project settings!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!string.IsNullOrEmpty(_targetTag) && other.CompareTag(_targetTag) && targetToAttack == null)
        {
            targetToAttack = other.transform;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!string.IsNullOrEmpty(_targetTag) && other.CompareTag(_targetTag) && targetToAttack == null)
        {
            targetToAttack = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!string.IsNullOrEmpty(_targetTag) && other.CompareTag(_targetTag) && targetToAttack != null)
        {
            targetToAttack = null;
        }
    }

    public void Attack()
    {
        if (targetToAttack != null)
        {
            Unit targetUnit = targetToAttack.GetComponent<Unit>();
            if (targetUnit != null)
            {
                targetUnit.TakeDamage(unitDamage);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}