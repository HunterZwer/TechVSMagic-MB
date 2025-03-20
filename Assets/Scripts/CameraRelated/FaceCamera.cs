using UnityEngine;

namespace CameraRelated
{
    public class FaceCamera : MonoBehaviour
    {
        private Transform _cameraTransform;
        private Transform _selfTransform;

        private void Awake()
        {
            // Cache references once
            _selfTransform = transform;
            CacheMainCamera();
        }

        private void CacheMainCamera()
        {
            // Find camera once using tag instead of Camera.main
            GameObject mainCamera = GameObject.FindWithTag("MainCamera");
            if(mainCamera) _cameraTransform = mainCamera.transform;
        }

        private void LateUpdate()
        {
            if(_cameraTransform is null) return;
        
            // Calculate rotation once per frame
            Vector3 directionToCamera = _selfTransform.position - _cameraTransform.position;
            _selfTransform.rotation = Quaternion.LookRotation(directionToCamera);
        }
    }
}