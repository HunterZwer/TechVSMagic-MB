using UnityEngine;
using System.Collections;

namespace CameraRelated
{
    public class SmoothCameraChange : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Tooltip("Duration of the movement in seconds")]
        public float moveDuration = 1f;
        [Tooltip("Animation curve for movement easing")]
        public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private Coroutine _activeCoroutine;
        private Vector3 _currentTarget;

        public void MoveCameraTo(Vector3 targetPosition)
        {
            // Keep the original Y position
            targetPosition.y = transform.position.y;
            
            // Don't restart if we're already moving to the same position
            if (_currentTarget == targetPosition) return;
            
            _currentTarget = targetPosition;

            // Stop any existing movement
            if (_activeCoroutine != null)
            {
                StopCoroutine(_activeCoroutine);
            }

            _activeCoroutine = StartCoroutine(SmoothMove(targetPosition));
        }

        private IEnumerator SmoothMove(Vector3 targetPosition)
        {
            Vector3 startPosition = transform.position;
            float elapsed = 0f;

            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = movementCurve.Evaluate(elapsed / moveDuration);
                transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            // Ensure final position is exact
            transform.position = targetPosition;
            _activeCoroutine = null;
        }

        public void StopMovement()
        {
            if (_activeCoroutine != null)
            {
                StopCoroutine(_activeCoroutine);
                _activeCoroutine = null;
            }
        }
    }
}