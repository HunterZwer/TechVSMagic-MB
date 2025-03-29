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
        [SerializeField] private KeyCode focusKey;
        [SerializeField] private float focusSpeed = 5f;
        [SerializeField] private Button focusButton;

        private bool isMoving = false;
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
            Unit closestUnit = FindClosestUnit();
            if (closestUnit != null)
            {
                SetTargetPosition(closestUnit);
            }
        }

        private Unit FindClosestUnit()
        {
            return FindObjectsOfType<Unit>()
                .OrderBy(unit => Vector3.SqrMagnitude(mainCamera.transform.position - unit.transform.position))
                .FirstOrDefault();
        }

        private void SetTargetPosition(Unit unit)
        {
            if (mainCamera != null && unit != null)
            {
                float sizeFactor = mainCamera.orthographicSize / baseSize;
                float adjustedDistance = baseDistance * sizeFactor;

                Vector3 unitPosition = unit.transform.position;
                targetPosition = unitPosition + new Vector3(-adjustedDistance, adjustedDistance * 1.3f, -adjustedDistance);
                isMoving = true;

                // Lock rotation
                mainCamera.transform.rotation = Quaternion.Euler(45f, 45f, 0f);

                // Move instantly (remove this if smooth movement is needed)
                mainCamera.transform.position = targetPosition;
                isMoving = false;
            }
        }
    }
}
