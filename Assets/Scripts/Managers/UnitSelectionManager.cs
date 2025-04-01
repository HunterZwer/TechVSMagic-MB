using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UnitSelectionManager : SelectionMethods
{
    public static UnitSelectionManager Instance { get; private set; }

    public List<GameObject> allUnitSelected = new List<GameObject>();
    
    public LayerMask clickable;
    public LayerMask ground;
    public LayerMask attackable;
    [Space][SerializeField] private Renderer ClickIcon;
    
    public bool attackCursorVisible;
    
    private Camera cam;
    private static readonly int CLCIK_TIME_PROPERTY = Shader.PropertyToID("_ClickTime");
    
    
    private float lastClickTime;
    private const float doubleClickThreshold = 0.3f; 

    public delegate void SelectionChanged();
    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        cam = Camera.main;
        Time.fixedDeltaTime = 0.05f;
        UpdateSelectionButtonText();
        _selectAllUnitsButton.onClick.AddListener(SelectAllPlayerUnits);
    }

    void Update()
    {
        HandleLeftClick();
        HandleRightClick();
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SelectAllPlayerUnits();
        }
    }
    
    
    private void HandleLeftClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // ��������� �������� �� ���� �� UI
            if (IsPointerOverUI()) return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, clickable))
            {
                GameObject clickedUnit = hit.collider.gameObject;
                float timeSinceLastClick = Time.time - lastClickTime;
                lastClickTime = Time.time;

                if (timeSinceLastClick <= doubleClickThreshold)
                {
                    SelectAllUnitsOfSameType(clickedUnit);
                }
                else if (Input.GetKey(KeyCode.LeftShift))
                {
                    MultiSelect(clickedUnit);
                }
                else
                {
                    SelectByClicking(clickedUnit);
                }
            }
            else if (!Input.GetKey(KeyCode.LeftShift))
            {
                DeselectAll();
            }
        }
    }

    private void HandleRightClick()
    {
        if (Input.GetMouseButtonDown(1))
        {
            // ��������� �������� �� ���� �� UI
            if (IsPointerOverUI()) return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ground))
            {
                if (unitSelected.Count > 0)
                {
                    ClickIcon.transform.position = hit.point + Vector3.up * 0.01f;
                    ClickIcon.material.SetFloat(CLCIK_TIME_PROPERTY, Time.time);
                }
            }

            if (_hasOffensiveUnits && Physics.Raycast(ray, out hit, Mathf.Infinity, attackable))
            {
                Transform target = hit.transform;
                foreach (var unitData in selectedUnitsWithAttack)
                {
                    if (unitData.meleeAttack != null)
                    {
                        unitData.meleeAttack.targetToAttack = target;
                    }
                    if (unitData.rangeAttack != null)
                    {
                        unitData.rangeAttack.targetToAttack = target;
                    }
                }
            }
        }
    }
    

    private bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
    
}
