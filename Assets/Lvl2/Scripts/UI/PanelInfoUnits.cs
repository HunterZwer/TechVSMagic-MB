using UnityEngine;
using TMPro;

public class PanelInfoUnits : MonoBehaviour
{
    [SerializeField] public GameObject _panelInfo;
    [SerializeField] private TextMeshProUGUI _name, _damage, _speed,_goldCost,_silverCost;
   


    public static PanelInfoUnits Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetActivePanelInfo(false);
    }

    public void SetInfo(string name, string damage, string speed, string goldCost, string silverCost)
    {
        _name.text = name;
        _damage.text = damage;
        _speed.text = speed;
        _goldCost.text = goldCost;
        _silverCost.text = silverCost;


    }

    public void SetActivePanelInfo(bool active) => _panelInfo.SetActive(active);

    public void TransformingPanel(float X) => _panelInfo.transform.position = new Vector3(X, _panelInfo.transform.position.y);

}
