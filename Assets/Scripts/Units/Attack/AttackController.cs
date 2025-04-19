using UnityEngine;
using UnityEngine.Serialization;

public class AttackController : MonoBehaviour
{
    public Transform targetToAttack;
    public float unitDamage;
    public string targetTag;
    public Unit ThisUnit;
    public UnitStats unitStats;
    public float baseDamage;
    public float baseRange;
    
    private void Awake()
    {
        ThisUnit = GetComponent<Unit>();
        unitStats = JsonLoader.LoadUnitStats(ThisUnit.unitClass, ThisUnit.IsPlayer);
        unitDamage = 10f;
        baseDamage = 10f;
        baseRange = 10f;
        
        targetTag = ThisUnit.IsPlayer ? "Enemy" : "Player";
    }
}