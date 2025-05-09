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
    // � ���� ������ ��������� ����� � �������� �
    var agent = _unitLvl2.GetComponent<NavMeshAgent>();
    string attackInfo = "";
    if (_unitLvl2.TryGetComponent<MeleeAttackController>(out var melee))
        attackInfo = $"{melee.unitDamage}";
    else if (_unitLvl2.TryGetComponent<RangeAttackController>(out var ranged))
        attackInfo = $"{ranged.unitDamage}";

   
  

    // ������� �� ������
    PanelInfoUnits.Instance.TransformingPanel(transform.position.x);
        PanelInfoUnits.Instance.SetInfo(
            _unitLvl2.name,
            $"����: {attackInfo}",
            $"��������: {agent.speed}",
            _unitLvl2._goldCost.ToString(),
            _unitLvl2._silverCost.ToString()

        );
    PanelInfoUnits.Instance.SetActivePanelInfo(true);
}




    public void OnPointerExit(PointerEventData eventData)
    {
        PanelInfoUnits.Instance.SetActivePanelInfo(false);
    }




}
