using UnityEngine;

namespace CameraRelated
{
    public class ZoomCamera : MonoBehaviour
    {
        [Header("Zoom Settings")]
        public float zoomSpeed = 5f; 
        public float minOrthoSize = 1f; 
        public float maxOrthoSize = 20f;
        
        private Camera _mainCamera;
    
        private void Awake()
        {
            CacheMainCamera();
        }

        private void CacheMainCamera()
        {
            GameObject mainCameraObj = GameObject.FindWithTag("MainCamera");
            if(mainCameraObj != null)
            {
                _mainCamera = mainCameraObj.GetComponent<Camera>();
                if(_mainCamera == null)
                {
                    Debug.LogError("Main camera object doesn't have Camera component!");
                }
            }
            else
            {
                Debug.LogError("No object with 'MainCamera' tag found!");
            }
        }

        private void LateUpdate()
        {
            if(!_mainCamera) return;
        
            HandleZoom();
        }
    
        private void HandleZoom()
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");

            if (scrollInput == 0) return;
            float newSize = _mainCamera.orthographicSize - scrollInput * zoomSpeed;
            _mainCamera.orthographicSize = Mathf.Clamp(newSize, minOrthoSize, maxOrthoSize);
        }
    }
}