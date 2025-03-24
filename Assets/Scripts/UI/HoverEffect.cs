using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;


public class HoverEffect : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
{

    [SerializeField] private GameObject _unitInfoShowPrefab;


    private UnitStats unitStats;
    private Unit _unit;
    private float unitDamage;
    private int _damageUprgadeLevel = 0;

    private void Start()
    {
        _unit = _unitInfoShowPrefab.GetComponent<Unit>();
        unitStats = JsonLoader.LoadUnitStats(_unit.unitClass, _unit.IsPlayer);
        unitDamage = unitDamage * unitStats.DamageMultiplier[_damageUprgadeLevel];
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

        

        Unit unitComponent = _unit.GetComponent<Unit>();
        unitComponent.TryGetComponent(out NavMeshAgent agent);
        string attackInfo = "";
        if (unitComponent.TryGetComponent(out MeleeAttackController melee))
            attackInfo = $"{melee.unitDamage}";
        else if (unitComponent.TryGetComponent(out RangeAttackController ranged))
            attackInfo = $" {ranged.projectileDamage}";
        PanelInfoUnits.Instance.TransformingPanel(transform.position.x);
        PanelInfoUnits.Instance.SetInfo($"Name: {_unit.name}", $"Damage: {attackInfo}", $"Speed {agent.speed}");
        PanelInfoUnits.Instance.SetActivePanelInfo(true);
    }



    public void OnPointerExit(PointerEventData eventData)
    {
        PanelInfoUnits.Instance.SetActivePanelInfo(false);
    }




}
