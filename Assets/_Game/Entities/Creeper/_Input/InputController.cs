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
            input.PlayerInput.MoveCreeper.performed += OnMove;
            input.PlayerInput.RotateCamera.performed += OnRotate;
            input.PlayerInput.ZoomCamera.performed += OnZoom;
            input.PlayerInput.Reset.performed += OnReset;

            input.PlayerInput.MoveCreeper.canceled += OnCancelMove;
            input.PlayerInput.RotateCamera.canceled += OnCancelRotate;
            input.PlayerInput.ZoomCamera.canceled += OnCancelZoom;
        }

        private void OnDisable()
        {
            input.Disable();
            input.PlayerInput.MoveCreeper.performed -= OnMove;
            input.PlayerInput.RotateCamera.performed -= OnRotate;
            input.PlayerInput.ZoomCamera.performed -= OnZoom;
            input.PlayerInput.Reset.performed -= OnReset;

            input.PlayerInput.MoveCreeper.canceled -= OnCancelMove;
            input.PlayerInput.RotateCamera.canceled -= OnCancelRotate;
            input.PlayerInput.ZoomCamera.canceled -= OnCancelZoom;
        }
        #endregion PlayerInputBinding

        #region PlayerInputReferences
        public HeadController Head;
        public BranchController Branch;
        public CameraController Camera;
        #endregion PlayerInputReferences

        public void OnMove(InputAction.CallbackContext _directionCallback)
        {
            var direction = _directionCallback.ReadValue<Vector2>();
            if (direction.magnitude > 1f)
            {
                direction.Normalize();
            }
            Head.InputDirection = direction;
            Branch.InputDirection = direction;
        }

        public void OnCancelMove(InputAction.CallbackContext _directionCallback)
        {
            Head.InputDirection = Vector3.zero;
            Branch.InputDirection = Vector3.zero;
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

        public void OnZoom(InputAction.CallbackContext _directionCallback)
        {
            var direction = _directionCallback.ReadValue<Vector3>();
            if (direction.magnitude > 1f)
            {
                direction.Normalize();
            }
        }

        public void OnCancelZoom(InputAction.CallbackContext _directionCallback)
        {
            var direction = _directionCallback.ReadValue<Vector3>();
            if (direction.magnitude > 1f)
            {
                direction.Normalize();
            }
        }

        public void OnReset(InputAction.CallbackContext _directionCallback)
        {
            if (IsReloadingScene) return;

            IsReloadingScene = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
