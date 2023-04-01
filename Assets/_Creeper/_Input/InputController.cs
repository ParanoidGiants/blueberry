using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Creeper
{
    public class InputController : MonoBehaviour
    {
        #region PlayerInputBinding
        private PlayerInputs input = null;
        private bool IsReloadingScene = false;

        private void Awake()
        {
            IsReloadingScene = false;
            input = new PlayerInputs();
        }

        private void OnEnable()
        {
            input.Enable();
            input.PlayerInput.Move.performed += OnMove;
            input.PlayerInput.Rotate.performed += OnRotate;
            input.PlayerInput.Reset.performed += OnReset;

            input.PlayerInput.Move.canceled += OnCancelMove;
            input.PlayerInput.Rotate.canceled += OnCancelRotate;
        }

        private void OnDisable()
        {
            input.Disable();
            input.PlayerInput.Move.performed -= OnMove;
            input.PlayerInput.Rotate.performed -= OnRotate;
            input.PlayerInput.Reset.performed -= OnReset;

            input.PlayerInput.Move.canceled -= OnCancelMove;
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

        public void OnCancelMove(InputAction.CallbackContext _directionCallback)
        {
            Head.SetMovementDirection(Vector3.zero);
        }

        public void OnRotate(InputAction.CallbackContext _directionCallback)
        {
            var direction = _directionCallback.ReadValue<Vector3>();
            if (direction.magnitude > 1f)
            {
                direction.Normalize();
            }
            Camera.SetRotateDirection(direction);
        }

        public void OnCancelRotate(InputAction.CallbackContext _directionCallback)
        {
            Camera.SetRotateDirection(Vector3.zero);
        }

        public void OnReset(InputAction.CallbackContext _directionCallback)
        {
            if (IsReloadingScene) return;

            IsReloadingScene = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
