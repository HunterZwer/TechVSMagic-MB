using UnityEngine;

public class AttackController : MonoBehaviour
{
    public Transform targetToAttack;
    public float unitDamage;
    public string targetTag;
    public Unit _unit;
    public UnitStats unitStats;
    private void Awake()
    {
        _unit = GetComponent<Unit>();
        unitStats = JsonLoader.LoadUnitStats(_unit.unitClass, _unit.IsPlayer);
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