using UnityEngine;
using System.Linq;
using UnityEngine.UI;

namespace CameraRelated
{
    public class UnitFocusCamera : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float baseSize = 5f;
        [SerializeField] private float baseDistance = 10f;
        [SerializeField] private KeyCode focusKey = KeyCode.F2;
        [SerializeField] private Button focusButton;
        
        private Vector3 targetPosition;

        private void Start()
        {
            mainCamera  = Camera.main;   
            focusButton.onClick.AddListener(MoveToClosestUnit);
        }

        private void Update()
        {
            if (Input.GetKeyDown(focusKey))
            {
                MoveToClosestUnit();
            }
        }

        private void MoveToClosestUnit()
        {
            UnitLVL2 closestUnitLvl2 = FindClosestUnit();
            if (closestUnitLvl2 != null)
            {
                SetTargetPosition(closestUnitLvl2);
            }
        }

        private UnitLVL2 FindClosestUnit()
        {
            return FindObjectsByType<UnitLVL2>(FindObjectsSortMode.None)
                .OrderBy(unit => Vector3.SqrMagnitude(mainCamera.transform.position - unit.transform.position))
                .FirstOrDefault();
        }

        private void SetTargetPosition(UnitLVL2 unitLvl2)
        {
            if (mainCamera != null && unitLvl2 != null)
            {
                float sizeFactor = mainCamera.orthographicSize / baseSize;
                float adjustedDistance = baseDistance * sizeFactor;

                Vector3 unitPosition = unitLvl2.transform.position;
                targetPosition = unitPosition + new Vector3(-adjustedDistance, adjustedDistance * 1.3f, -adjustedDistance);

                // Lock rotation
                mainCamera.transform.rotation = Quaternion.Euler(45f, 45f, 0f);

                // Move instantly (remove this if smooth movement is needed)
                mainCamera.transform.position = targetPosition;
            }
        }
    }
}
