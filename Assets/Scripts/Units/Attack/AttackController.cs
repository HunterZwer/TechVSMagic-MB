using UnityEngine;
using UnityEngine.Serialization;

public class AttackController : MonoBehaviour
{
    public Transform targetToAttack;
    public float unitDamage;
    public string targetTag;
    [FormerlySerializedAs("ThisUnit")] public UnitLVL2 thisUnitLvl2;
    public UnitStats unitStats;
    public float baseDamage;
    public float baseRange;
    
    private void Awake()
    {
        thisUnitLvl2 = GetComponent<UnitLVL2>();
        unitStats = JsonLoader.LoadUnitStats(thisUnitLvl2.unitClass, thisUnitLvl2.IsPlayer);
        unitDamage = 10f;
        baseDamage = 10f;
        baseRange = 10f;
        
        targetTag = thisUnitLvl2.IsPlayer ? "Enemy" : "Player";
    }
}