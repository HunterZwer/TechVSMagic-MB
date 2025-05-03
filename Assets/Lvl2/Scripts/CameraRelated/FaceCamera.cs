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
        }
        

        private void LateUpdate()
        {
            _selfTransform.rotation = new Quaternion(0.32612f, 0.36653f,-0.13895f, 0.86023f);
        }
    }
}