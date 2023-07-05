using Roots;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Creeper
{
    public class InputController : MonoBehaviour
    {
        private PlayerInputs _input;
        private bool _isReloadingScene;
        private bool _areInputsFrozen;

        public HeadController head;
        public VineController vine;
        public CameraController cameraController;

        private FertilizerManager _fertilizerManager;
        private FertilizerManager fertilizerManager {
            get
            {
                if (_fertilizerManager == null)
                {
                    _fertilizerManager = FindObjectOfType<FertilizerManager>();
                }
                return _fertilizerManager;
            }
        }

        private void Awake()
        {
            _isReloadingScene = false;
            _input = new PlayerInputs();
        }

        private void OnEnable()
        {
            _input.Enable();
            _input.PlayerInput.MoveCreeper.performed += OnMove;
            _input.PlayerInput.ZoomCamera.performed += OnZoom;
            _input.PlayerInput.Reset.performed += OnReset;
            _input.PlayerInput.Confirm.performed += OnConfirm;

            _input.PlayerInput.MoveCreeper.canceled += OnCancelMove;
            _input.PlayerInput.ZoomCamera.canceled += OnCancelZoom;
        }

        private void OnDisable()
        {
            _input.Disable();
            _input.PlayerInput.MoveCreeper.performed -= OnMove;
            _input.PlayerInput.ZoomCamera.performed -= OnZoom;
            _input.PlayerInput.Reset.performed -= OnReset;
            _input.PlayerInput.Confirm.performed -= OnConfirm;

            _input.PlayerInput.MoveCreeper.canceled -= OnCancelMove;
            _input.PlayerInput.ZoomCamera.canceled -= OnCancelZoom;
        }

        private void OnConfirm(InputAction.CallbackContext obj)
        {
            if (fertilizerManager == null) return;
            
            if (fertilizerManager.IsDelivered)
            {
                Game.Instance.OnLoadFirstLevel();
            }
                
        }

        public void OnMove(InputAction.CallbackContext _directionCallback)
        {
            if (_areInputsFrozen) return;
            
            var direction = _directionCallback.ReadValue<Vector2>();
            if (direction.magnitude > 1f)
            {
                direction.Normalize();
            }
            head.InputDirection = direction;
            vine.InputDirection = direction;
        }

        public void OnCancelMove(InputAction.CallbackContext _directionCallback)
        {
            if (_areInputsFrozen) return;

            head.InputDirection = Vector3.zero;
            vine.InputDirection = Vector3.zero;
        }
        public void OnZoom(InputAction.CallbackContext _directionCallback)
        {
            if (_areInputsFrozen) return;

            var direction = _directionCallback.ReadValue<float>();
            cameraController.SetZoomDirection(direction);
        }

        public void OnCancelZoom(InputAction.CallbackContext _directionCallback)
        {
            if (_areInputsFrozen) return;

            var direction = _directionCallback.ReadValue<float>();
            cameraController.SetZoomDirection(direction);
        }

        public void OnReset(InputAction.CallbackContext _directionCallback)
        {
            if (_isReloadingScene) return;

            _isReloadingScene = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void FreezeInputs()
        {
            _areInputsFrozen = true;
        }

        public void UnfreezeInputs()
        {
            _areInputsFrozen = false;
        }
    }
}
