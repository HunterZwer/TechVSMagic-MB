using UnityEngine;

namespace CameraRelated
{
    public class ZoomCamera : MonoBehaviour
    {
        [Header("Zoom Settings")]
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float minOrthoSize = 1f;
        [SerializeField] private float maxOrthoSize = 20f;
        [SerializeField] private bool useSmoothing = true;
        [SerializeField] private float smoothingSpeed = 8f;
        
        private Camera _mainCamera;
        private float _targetOrthoSize;
        
        private void Awake()
        {
            _mainCamera = Camera.main;
            if (!_mainCamera) {Debug.LogError("No main camera");}
            _targetOrthoSize = _mainCamera.orthographicSize;
        }
        
        private void LateUpdate()
        {
            HandleZoom();
        }
        
        private void HandleZoom()
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            
            if (scrollInput != 0)
            {
                // Update target size based on scroll input
                float dynamicZoomSpeed = zoomSpeed * (_targetOrthoSize / maxOrthoSize);
                _targetOrthoSize -= scrollInput * dynamicZoomSpeed;
                _targetOrthoSize = Mathf.Clamp(_targetOrthoSize, minOrthoSize, maxOrthoSize);
            }
            
            // Apply zoom - either immediate or smooth
            if (useSmoothing)
            {
                _mainCamera.orthographicSize = Mathf.Lerp(
                    _mainCamera.orthographicSize, 
                    _targetOrthoSize, 
                    Time.deltaTime * smoothingSpeed
                );
            }
            else if (_mainCamera.orthographicSize != _targetOrthoSize)
            {
                _mainCamera.orthographicSize = _targetOrthoSize;
            }
        }
        
        // Public method to set zoom programmatically
        public void SetZoom(float newZoomLevel)
        {
            if (_mainCamera == null) return;
            
            _targetOrthoSize = Mathf.Clamp(newZoomLevel, minOrthoSize, maxOrthoSize);
        }
    }
}