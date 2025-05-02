using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;


public class HoverEffect : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
{

    [SerializeField] private GameObject _unitInfoShowPrefab;


    private UnitStats unitStats;
    private UnitLVL2 _unitLvl2;
    private float unitDamage;
    private int _damageUprgadeLevel = 0;

    private void Start()
    {
        _unitLvl2 = _unitInfoShowPrefab.GetComponent<UnitLVL2>();
        unitStats = JsonLoader.LoadUnitStats(_unitLvl2.unitClass, _unitLvl2.IsPlayer);
        unitDamage = unitDamage * unitStats.DamageMultiplier[_damageUprgadeLevel];
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

        

        UnitLVL2 unitLvl2Component = _unitLvl2.GetComponent<UnitLVL2>();
        unitLvl2Component.TryGetComponent(out NavMeshAgent agent);
        string attackInfo = "";
        if (unitLvl2Component.TryGetComponent(out MeleeAttackController melee))
            attackInfo = $"{melee.unitDamage}";
        else if (unitLvl2Component.TryGetComponent(out RangeAttackController ranged))
            attackInfo = $" {ranged.unitDamage}";
        PanelInfoUnits.Instance.TransformingPanel(transform.position.x);
        PanelInfoUnits.Instance.SetInfo($"Name: {_unitLvl2.name}", $"Damage: {attackInfo}", $"Speed {agent.speed}");
        PanelInfoUnits.Instance.SetActivePanelInfo(true);
    }



    public void OnPointerExit(PointerEventData eventData)
    {
        PanelInfoUnits.Instance.SetActivePanelInfo(false);
    }




}
