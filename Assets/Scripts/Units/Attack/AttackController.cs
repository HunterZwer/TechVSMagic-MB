using UnityEngine;
using UnityEngine.Serialization;

public class AttackController : MonoBehaviour
{
    public Transform targetToAttack;
    public float unitDamage;
    public string targetTag;
    public Unit ThisUnit;
    public UnitStats unitStats;
    private void Awake()
    {
        ThisUnit = GetComponent<Unit>();
        unitStats = JsonLoader.LoadUnitStats(ThisUnit.unitClass, ThisUnit.IsPlayer);
    }
    public bool IsTargetDead(Transform targetToAttack)
    {
        if (targetToAttack)
        {
            targetToAttack.TryGetComponent(out Unit unit);
            return unit is null || !unit.IsDead;
        }
        return true;
    }
    
    public string SetTargetTag(Unit unit)
    {
        return unit.IsPlayer ? "Enemy" : "Player";
    }

}