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
            CacheMainCamera();
            
            if (_mainCamera != null)
                _targetOrthoSize = _mainCamera.orthographicSize;
        }
        
        private void CacheMainCamera()
        {
            // Try Camera.main first, which is more efficient than FindWithTag
            _mainCamera = Camera.main;
            
            // Fallback to FindWithTag if Camera.main fails
            if (_mainCamera == null)
            {
                var mainCameraObj = GameObject.FindWithTag("MainCamera");
                if (mainCameraObj != null)
                {
                    _mainCamera = mainCameraObj.GetComponent<Camera>();
                    if (_mainCamera == null)
                        Debug.LogWarning("Main camera object doesn't have Camera component!");
                }
                else
                {
                    Debug.LogWarning("No object with 'MainCamera' tag found!");
                }
            }
        }
        
        private void LateUpdate()
        {
            if (_mainCamera == null)
            {
                CacheMainCamera();
                if (_mainCamera == null) return;
                
                _targetOrthoSize = _mainCamera.orthographicSize;
            }
            
            HandleZoom();
        }
        
        private void HandleZoom()
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            
            if (scrollInput != 0)
            {
                // Update target size based on scroll input
                _targetOrthoSize -= scrollInput * zoomSpeed;
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