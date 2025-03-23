using UnityEngine;

namespace CameraRelated
{
    public class EdgeScroller : MonoBehaviour
    {
        [SerializeField] private float movementSensitivity = 1f;
        [SerializeField] private float fastSpeed = 4f;
        [SerializeField] private float normalSpeed = 2f;
        
        private CursorArrow _currentCursor = CursorArrow.DEFAULT;
        public Texture2D cursorArrowUp;
        public Texture2D cursorArrowDown;
        public Texture2D cursorArrowLeft;
        public Texture2D cursorArrowRight;
        
        private Vector3 _newPosition;
        private bool _isCursorSet;
        private const float EdgeSize = 50f;
        private float _movementSpeed;

        enum CursorArrow
        {
            UP,
            DOWN,
            LEFT,
            RIGHT,
            DEFAULT
        }

        private void LateUpdate()
        {
            EdgeScrollMovement();
        }

        void EdgeScrollMovement()
        {
            
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                _movementSpeed = fastSpeed;
            }
            else
            {
                _movementSpeed = normalSpeed;
            }
            _newPosition = transform.position;
            // Move Right
            if (Input.mousePosition.x > Screen.width - EdgeSize)
            {
                _newPosition += (transform.right * _movementSpeed);
                ChangeCursor(CursorArrow.RIGHT);
                _isCursorSet = true;
            }

            // Move Left
            else if (Input.mousePosition.x < EdgeSize)
            {
                _newPosition += (transform.right * -_movementSpeed);
                ChangeCursor(CursorArrow.LEFT);
                _isCursorSet = true;
            }

            // Move Up
            else if (Input.mousePosition.y > Screen.height - EdgeSize)
            {
                _newPosition += (transform.forward * _movementSpeed);
                ChangeCursor(CursorArrow.UP);
                _isCursorSet = true;
            }

            // Move Down
            else if (Input.mousePosition.y < EdgeSize)
            {
                _newPosition += (transform.forward * -_movementSpeed);
                ChangeCursor(CursorArrow.DOWN);
                _isCursorSet = true;
            }
            else
            {
                _newPosition = transform.position;
                if (_isCursorSet)
                {
                    ChangeCursor(CursorArrow.DEFAULT);
                    _isCursorSet = false;
                }
            }

            transform.position = Vector3.Lerp(transform.position, _newPosition, Time.deltaTime * movementSensitivity);

            Cursor.lockState =
                CursorLockMode.Confined; // If we have an extra monitor we don't want to exit screen bounds
        }

        void ChangeCursor(CursorArrow newCursor)
        {
            // Only change cursor if it's not the same cursor
            if (_currentCursor != newCursor)
            {
                switch (newCursor)
                {
                    case CursorArrow.UP:
                        Cursor.SetCursor(cursorArrowUp, Vector2.zero, CursorMode.Auto);
                        break;
                    case CursorArrow.DOWN:
                        Cursor.SetCursor(cursorArrowDown,
                            new Vector2(cursorArrowDown.width, cursorArrowDown.height),
                            CursorMode.Auto); // So the Cursor will stay inside view
                        break;
                    case CursorArrow.LEFT:
                        Cursor.SetCursor(cursorArrowLeft, Vector2.zero, CursorMode.Auto);
                        break;
                    case CursorArrow.RIGHT:
                        Cursor.SetCursor(cursorArrowRight,
                            new Vector2(cursorArrowRight.width, cursorArrowRight.height),
                            CursorMode.Auto); // So the Cursor will stay inside view
                        break;
                    case CursorArrow.DEFAULT:
                        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                        break;
                }

                _currentCursor = newCursor;
            }
        }
    }
}