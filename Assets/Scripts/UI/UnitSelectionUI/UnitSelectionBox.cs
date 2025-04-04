using System.Collections.Generic;
using UnityEngine;

public class UnitSelectionBox : MonoBehaviour
{
    [SerializeField] private Camera myCam;
    [SerializeField] private RectTransform boxVisual;
    
    private Rect selectionBox;
    private Vector2 startPosition;
    private Vector2 endPosition;
    private bool isSelecting = false;
    
    private void Start()
    {
        if (myCam == null)
            myCam = Camera.main;
        boxVisual.gameObject.SetActive(false);
    }
    
    private void Update()
    {
        // Only perform selection logic when left mouse button is used
        if (Input.GetMouseButtonDown(0))
        {
            isSelecting = true;
            startPosition = Input.mousePosition;
            selectionBox = new Rect();
        }
        else if (Input.GetMouseButton(0) && isSelecting)
        {
            endPosition = Input.mousePosition;
            UpdateSelectionBox();
        }
        else if (Input.GetMouseButtonUp(0) && isSelecting)
        {
            if (HasMinimumSize())
            {
                UnitSelectionManager.Instance.DeselectAll(); // Deselect everything first
                SelectUnits(); // Then select new ones
            }
    
            isSelecting = false;
            boxVisual.gameObject.SetActive(false);
        }
    }
    
    private void UpdateSelectionBox()
    {
        // Update visual representation
        UpdateVisual();
        
        // Update selection rect coordinates
        selectionBox.xMin = Mathf.Min(startPosition.x, endPosition.x);
        selectionBox.xMax = Mathf.Max(startPosition.x, endPosition.x);
        selectionBox.yMin = Mathf.Min(startPosition.y, endPosition.y);
        selectionBox.yMax = Mathf.Max(startPosition.y, endPosition.y);
        
        // Only deselect and reselect if we have a meaningful selection size
        if (HasMinimumSize())
        {
            UnitSelectionManager.Instance.DeselectAll();
            SelectUnits();
        }
    }
    
    private void UpdateVisual()
    {
        Vector2 newBoxCenter = (startPosition + endPosition) * 0.5f;
        Vector2 newBoxSize = new Vector2(
            Mathf.Abs(endPosition.x - startPosition.x),
            Mathf.Abs(endPosition.y - startPosition.y)
        );

        if (boxVisual.position != (Vector3)newBoxCenter || boxVisual.sizeDelta != newBoxSize)
        {
            boxVisual.position = newBoxCenter;
            boxVisual.sizeDelta = newBoxSize;
            boxVisual.gameObject.SetActive(HasMinimumSize());
        }
    }

    
    private bool HasMinimumSize()
    {
        const float minSize = 5f; // Minimum size to consider a valid drag selection
        return Vector2.Distance(startPosition, endPosition) >= minSize;
    }
    
    private void SelectUnits()
    {
        var units = UnitSelectionManager.Instance.allUnitSelected;
    
        foreach (var unit in units)
        {
            if (unit != null)
            {
                Vector3 screenPosition = myCam.WorldToScreenPoint(unit.transform.position);
                if (selectionBox.Contains(screenPosition))
                {
                    UnitSelectionManager.Instance.DragSelect(unit);
                }
            }
        }
    }

}