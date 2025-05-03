using System;
using UnityEngine;

namespace CameraRelated
{
    public class EdgeScroller : MonoBehaviour
    {
        [SerializeField] private float movementSensitivity = 1f;
        [SerializeField] private float fastSpeed = 4f;
        [SerializeField] private float normalSpeed = 2f;
        [SerializeField] public float _maxSpeedMultiplier = 10f;

        // Cursor textures for cardinal directions.
        public Texture2D cursorArrowUp;
        public Texture2D cursorArrowDown;
        public Texture2D cursorArrowLeft;
        public Texture2D cursorArrowRight;
        // Cursor textures for diagonal directions.
        public Texture2D cursorArrowUpLeft;
        public Texture2D cursorArrowUpRight;
        public Texture2D cursorArrowDownLeft;
        public Texture2D cursorArrowDownRight;

        private Vector3 _newPosition;
        private const float EdgeSize = 50f;
        private float _movementSpeed;

        enum CursorArrow
        {
            UP,
            DOWN,
            LEFT,
            RIGHT,
            UP_LEFT,
            UP_RIGHT,
            DOWN_LEFT,
            DOWN_RIGHT,
            DEFAULT
        }

        private CursorArrow _currentCursor = CursorArrow.DEFAULT;

        private void LateUpdate()
        {
            EdgeScrollMovement();
        }

        void EdgeScrollMovement()
        {
            // Flags for edge detection.
            float distanceRight = Screen.width - Input.mousePosition.x;
            float distanceLeft = Input.mousePosition.x;
            float distanceUp = Screen.height - Input.mousePosition.y;
            float distanceDown = Input.mousePosition.y;
            
            float speedMultiplier = 1f;

// Calculate speed multiplier based on proximity to edge.
            if (distanceRight < EdgeSize)
                speedMultiplier = Mathf.Lerp(1f, _maxSpeedMultiplier, 1 - (distanceRight / EdgeSize));
            else if (distanceLeft < EdgeSize)
                speedMultiplier = Mathf.Lerp(1f, _maxSpeedMultiplier, 1 - (distanceLeft / EdgeSize));
            else if (distanceUp < EdgeSize)
                speedMultiplier = Mathf.Lerp(1f, _maxSpeedMultiplier, 1 - (distanceUp / EdgeSize));
            else if (distanceDown < EdgeSize)
                speedMultiplier = Mathf.Lerp(1f, _maxSpeedMultiplier, 1 - (distanceDown / EdgeSize));

            _movementSpeed = ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? fastSpeed : normalSpeed) * speedMultiplier;
            
            // Flags for edge detection.
            bool moveRight = Input.mousePosition.x > Screen.width - EdgeSize;
            bool moveLeft = Input.mousePosition.x < EdgeSize;
            bool moveUp = Input.mousePosition.y > Screen.height - EdgeSize;
            bool moveDown = Input.mousePosition.y < EdgeSize;

            // Calculate movement direction.
            Vector3 movementDir = Vector3.zero;
            if (moveRight)
                movementDir += transform.right;
            if (moveLeft)
                movementDir -= transform.right;
            if (moveUp)
                movementDir += transform.forward;
            if (moveDown)
                movementDir -= transform.forward;

            // Determine which cursor to display based on the combination of directions.
            if (moveUp && moveRight)
                ChangeCursor(CursorArrow.UP_RIGHT);
            else if (moveUp && moveLeft)
                ChangeCursor(CursorArrow.UP_LEFT);
            else if (moveDown && moveRight)
                ChangeCursor(CursorArrow.DOWN_RIGHT);
            else if (moveDown && moveLeft)
                ChangeCursor(CursorArrow.DOWN_LEFT);
            else if (moveUp)
                ChangeCursor(CursorArrow.UP);
            else if (moveDown)
                ChangeCursor(CursorArrow.DOWN);
            else if (moveRight)
                ChangeCursor(CursorArrow.RIGHT);
            else if (moveLeft)
                ChangeCursor(CursorArrow.LEFT);
            else
            {
                ChangeCursor(CursorArrow.DEFAULT);
            }

            // Normalize the movement vector to ensure consistent speed diagonally.
            if (movementDir != Vector3.zero)
            {
                movementDir.Normalize();
                _newPosition = transform.position + movementDir * _movementSpeed;
            }
            else
            {
                _newPosition = transform.position;
            }

            transform.position = Vector3.Lerp(transform.position, _newPosition, Time.deltaTime * movementSensitivity);
            Cursor.lockState = CursorLockMode.Confined;
        }

        void ChangeCursor(CursorArrow newCursor)
        {
            // Only update the cursor if it differs from the current one.
            if (_currentCursor != newCursor)
            {
                switch (newCursor)
                {
                    case CursorArrow.UP:
                        Cursor.SetCursor(cursorArrowUp, Vector2.zero, CursorMode.Auto);
                        break;
                    case CursorArrow.DOWN:
                        Cursor.SetCursor(cursorArrowDown, new Vector2(cursorArrowDown.width, cursorArrowDown.height), CursorMode.Auto);
                        break;
                    case CursorArrow.LEFT:
                        Cursor.SetCursor(cursorArrowLeft, Vector2.zero, CursorMode.Auto);
                        break;
                    case CursorArrow.RIGHT:
                        Cursor.SetCursor(cursorArrowRight, new Vector2(cursorArrowRight.width, cursorArrowRight.height), CursorMode.Auto);
                        break;
                    case CursorArrow.UP_LEFT:
                        Cursor.SetCursor(cursorArrowUpLeft, Vector2.zero, CursorMode.Auto);
                        break;
                    case CursorArrow.UP_RIGHT:
                        Cursor.SetCursor(cursorArrowUpRight, new Vector2(cursorArrowUpRight.width, cursorArrowUpRight.height), CursorMode.Auto);
                        break;
                    case CursorArrow.DOWN_LEFT:
                        Cursor.SetCursor(cursorArrowDownLeft, Vector2.zero, CursorMode.Auto);
                        break;
                    case CursorArrow.DOWN_RIGHT:
                        Cursor.SetCursor(cursorArrowDownRight, new Vector2(cursorArrowDownRight.width, cursorArrowDownRight.height), CursorMode.Auto);
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