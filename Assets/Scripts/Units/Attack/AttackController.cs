using UnityEngine;

public class AttackController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform targetToAttack;

    public Material idleStateMaterial;
    public Material followStateMaterial;
    public Material attackStateMaterial;

    public bool isPlayer;
    public int unitDamage;

    private void OnTriggerEnter(Collider other)
    {
        if (isPlayer && other.CompareTag("Enemy") && targetToAttack == null)
        {
            targetToAttack = other.transform;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isPlayer && other.CompareTag("Enemy") && targetToAttack == null)
        {
            targetToAttack = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isPlayer && other.CompareTag("Enemy") && targetToAttack != null)
        {
            targetToAttack = null;
        }
    }

    public void SetIdleMaterial()
    {
        GetComponent<Renderer>().material = idleStateMaterial;
    }
    
    public void SetFollowMaterial()
    {
        GetComponent<Renderer>().material = followStateMaterial;
    }
    
    public void SetAttackMaterial()
    {
        GetComponent<Renderer>().material = attackStateMaterial;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 10f*0.2f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position,1.2f);
    }
}