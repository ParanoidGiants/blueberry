using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Creeper
{
    public class InputController : MonoBehaviour
    {
        #region PlayerInputBinding
        private PlayerInputs input = null;

        private void Awake()
        {
            input = new PlayerInputs();
        }

        private void OnEnable()
        {
            input.Enable();
            input.PlayerInput.Move.performed += OnMove;
            input.PlayerInput.Move.canceled += OnMove;
            input.PlayerInput.Rotate.performed += OnRotate;
            input.PlayerInput.Rotate.canceled += OnCancelRotate;
        }

        private void OnDisable()
        {
            input.Disable();
            input.PlayerInput.Move.performed -= OnMove;
            input.PlayerInput.Move.canceled -= OnMove;
            input.PlayerInput.Rotate.performed -= OnRotate;
            input.PlayerInput.Rotate.canceled -= OnCancelRotate;
        }
        #endregion PlayerInputBinding

        #region PlayerInputReferences
        public HeadController Head;
        public CameraController Camera;
        #endregion PlayerInputReferences

        public void OnMove(InputAction.CallbackContext _directionCallback)
        {
            var direction = _directionCallback.ReadValue<Vector2>();
            if (direction.magnitude > 1f)
            {
                direction.Normalize();
            }
            Head.SetMovementDirection(direction);
        }

        public void OnRotate(InputAction.CallbackContext _directionCallback)
        {
            var direction = _directionCallback.ReadValue<Vector2>();
            if (direction.Equals(Vector3.zero)) return;
            if (direction.magnitude > 1f)
            {
                direction.Normalize();
            }
            Camera.SetRotateDirection(direction );
        }

        public void OnCancelRotate(InputAction.CallbackContext _directionCallback)
        {
            Camera.SetRotateDirection(Vector3.zero);
        }
    }
}
