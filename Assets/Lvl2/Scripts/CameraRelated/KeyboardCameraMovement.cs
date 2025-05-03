using UnityEngine;

namespace CameraRelated
{
    public class KeyboardCameraMovement : MonoBehaviour
    {
        [SerializeField] private float fastSpeed = 4f;
        [SerializeField] private float normalSpeed = 2f;
        [SerializeField] private float movementSensitivity = 1f;
        private float _movementSpeed;
        private Vector3 _newPosition;

        void Start()
        {
            _newPosition = transform.position;
        }
        
        void LateUpdate()
        {
            KeyboardMovement();
        }

        void KeyboardMovement()
        {
            _newPosition = transform.position;
            
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                _movementSpeed = fastSpeed;
            }
            else
            {
                _movementSpeed = normalSpeed;
            }
            
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                _newPosition += (transform.forward * (_movementSpeed * Time.deltaTime));
            }

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                _newPosition += (transform.forward * (-_movementSpeed * Time.deltaTime));
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                _newPosition += (transform.right * (_movementSpeed * Time.deltaTime));
            }

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                _newPosition += (transform.right * (-_movementSpeed * Time.deltaTime));
            }
            
            transform.position = Vector3.Lerp(transform.position, _newPosition, movementSensitivity);
        }
    }
}