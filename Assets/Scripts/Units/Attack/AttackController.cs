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
        unitDamage = 10f;
        targetTag = ThisUnit.IsPlayer ? "Enemy" : "Player";
    }

    protected bool IsTargetDead(Transform target) => 
        !target || !target.TryGetComponent(out Unit unit) || unit.IsDead;
}