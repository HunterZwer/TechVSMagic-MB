using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelectionBox : MonoBehaviour
{
    Camera myCam;

    [SerializeField]
    RectTransform boxVisual; // The UI element for the selection box

    Rect selectionBox; // The actual selection box in screen coordinates

    Vector2 startPosition; // Starting position of the selection box
    Vector2 endPosition; // Ending position of the selection box

    private void Start()
    {
        myCam = Camera.main;
        startPosition = Vector2.zero;
        endPosition = Vector2.zero;
        DrawVisual();
    }

    private void Update()
    {
        // When Clicked
        if (Input.GetMouseButtonDown(0))
        {
            startPosition = Input.mousePosition;
            selectionBox = new Rect(); // Reset the selection box
        }

        // When Dragging
        if (Input.GetMouseButton(0))
        {
            endPosition = Input.mousePosition;
            DrawVisual();
            DrawSelection();

            // Deselect all units and select new ones
            UnitSelectionManager.Instance.DeselectAll();
            SelectUnits();
        }

        // When Releasing
        if (Input.GetMouseButtonUp(0))
        {
            SelectUnits();

            // Reset the selection box
            startPosition = Vector2.zero;
            endPosition = Vector2.zero;
            DrawVisual();
        }
    }

    void DrawVisual()
    {
        // Calculate the center and size of the selection box
        Vector2 boxCenter = (startPosition + endPosition) / 2;
        Vector2 boxSize = new Vector2(Mathf.Abs(startPosition.x - endPosition.x), Mathf.Abs(startPosition.y - endPosition.y));

        // Update the position and size of the visual box
        boxVisual.position = boxCenter;
        boxVisual.sizeDelta = boxSize;

        // Show or hide the visual box based on whether the mouse is being dragged
        boxVisual.gameObject.SetActive(startPosition != Vector2.zero && endPosition != Vector2.zero);
    }

    void DrawSelection()
    {
        // Calculate the selection box boundaries
        if (Input.mousePosition.x < startPosition.x)
        {
            selectionBox.xMin = Input.mousePosition.x;
            selectionBox.xMax = startPosition.x;
        }
        else
        {
            selectionBox.xMin = startPosition.x;
            selectionBox.xMax = Input.mousePosition.x;
        }

        if (Input.mousePosition.y < startPosition.y)
        {
            selectionBox.yMin = Input.mousePosition.y;
            selectionBox.yMax = startPosition.y;
        }
        else
        {
            selectionBox.yMin = startPosition.y;
            selectionBox.yMax = Input.mousePosition.y;
        }
    }

    void SelectUnits()
    {
        // Select units within the selection box
        foreach (var unit in UnitSelectionManager.Instance.allUnitSelected)
        {
            Vector2 screenPosition = myCam.WorldToScreenPoint(unit.transform.position);
            if (selectionBox.Contains(screenPosition))
            {
                UnitSelectionManager.Instance.DragSelect(unit);
            }
        }
    }
}