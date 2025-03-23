using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace CameraRelated
{
public class OffScreenUnitIndicator : MonoBehaviour
{
    public RectTransform indicatorPrefab;
    public float edgePadding = 50f;
    public float moveDuration = 1f;
    public KeyCode hotkey = KeyCode.F2;

    private Transform cameraHolder;
    private Camera mainCamera;
    private Dictionary<Unit, float> offScreenTimes = new Dictionary<Unit, float>();
    private List<Unit> allUnits = new List<Unit>();
    private RectTransform indicatorInstance;
    private Unit closestUnit;
    private bool buttonVisible;

    void Start()
    {
        // Find camera holder and get camera reference
        cameraHolder = GameObject.FindGameObjectWithTag("CameraHolder").transform;
        mainCamera = cameraHolder.GetComponentInChildren<Camera>();
        
        // Initialize units
        Unit[] existingUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);

        allUnits.AddRange(existingUnits);
        
        // Create and hide indicator
        indicatorInstance = Instantiate(indicatorPrefab, transform);
        indicatorInstance.gameObject.SetActive(false);
        
        // Add click listener
        indicatorInstance.GetComponent<Button>().onClick.AddListener(OnIndicatorClicked);
    }

    void LateUpdate()
    {
        UpdateOffScreenTimers();
        UpdateIndicatorPosition();
        CheckHotkeyInput();
    }

    void UpdateOffScreenTimers()
    {
        foreach (Unit unit in allUnits)
        {
            if (unit == null || unit.IsDead)
            {
                offScreenTimes.Remove(unit);
                continue;
            }

            bool isVisible = IsUnitVisible(unit);
            
            if (isVisible)
            {
                offScreenTimes.Remove(unit);
            }
            else
            {
                if (offScreenTimes.ContainsKey(unit))
                {
                    offScreenTimes[unit] += Time.deltaTime;
                }
                else
                {
                    offScreenTimes[unit] = 0f;
                }
            }
        }

        UpdateButtonVisibility();
    }

    bool IsUnitVisible(Unit unit)
    {
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(unit.transform.position);
        return viewportPos.x > 0 && viewportPos.x < 1 && 
               viewportPos.y > 0 && viewportPos.y < 1 && 
               viewportPos.z > 0;
    }

    void UpdateButtonVisibility()
    {
        bool shouldShow = false;
        closestUnit = null;
        float closestDistance = Mathf.Infinity;

        foreach (var pair in offScreenTimes)
        {
            if (pair.Value >= 2f)
            {
                float dist = Vector3.Distance(
                    cameraHolder.position, 
                    pair.Key.transform.position
                );

                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestUnit = pair.Key;
                    shouldShow = true;
                }
            }
        }

        if (buttonVisible != shouldShow)
        {
            indicatorInstance.gameObject.SetActive(shouldShow);
            buttonVisible = shouldShow;
        }
    }

    void UpdateIndicatorPosition()
    {
        if (!buttonVisible || closestUnit == null) return;

        Vector3 screenPos = mainCamera.WorldToScreenPoint(closestUnit.transform.position);
        
        // Handle behind-camera case
        if (screenPos.z < 0)
        {
            screenPos *= -1;
        }

        // Clamp to screen edges
        Vector3 clampedPos = new Vector3(
            Mathf.Clamp(screenPos.x, edgePadding, Screen.width - edgePadding),
            Mathf.Clamp(screenPos.y, edgePadding, Screen.height - edgePadding),
            screenPos.z
        );

        indicatorInstance.position = clampedPos;
    }

    void CheckHotkeyInput()
    {
        if (Input.GetKeyDown(hotkey))
        {
            MoveToClosestUnit();
        }
    }

    public void OnIndicatorClicked()
    {
        MoveToClosestUnit();
        // Immediately hide button after click
        indicatorInstance.gameObject.SetActive(false);
        buttonVisible = false;
    }

    void MoveToClosestUnit()
    {
        if (closestUnit == null) return;

        StopAllCoroutines();
        StartCoroutine(SmoothCameraMove(closestUnit.transform.position));
    }

    IEnumerator SmoothCameraMove(Vector3 targetPosition)
    {
        Vector3 cameraOffset = new Vector3(-187f, 0f, -183f);
        Vector3 targetHolderPosition = targetPosition + cameraOffset;
    
        float elapsed = 0f;
        Vector3 startPos = cameraHolder.position;

        while (elapsed < moveDuration)
        {
            cameraHolder.position = Vector3.Lerp(
                startPos, 
                targetHolderPosition, 
                elapsed / moveDuration
            );
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        targetHolderPosition.y = 0;
        cameraHolder.position = targetHolderPosition;
    }

    public void RegisterUnit(Unit unit) => allUnits.Add(unit);
    public void UnregisterUnit(Unit unit) => allUnits.Remove(unit);
}
}