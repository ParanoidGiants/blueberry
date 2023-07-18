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
        private bool _isInputFrozen;

        private HeadController _head;
        private MeshGenerator _meshGenerator;

        private CollectableFetrilizer.FertilizerManager _fertilizerManager;
        private CollectableFetrilizer.FertilizerManager FertilizerManager
        {
            get
            {
                if (_fertilizerManager == null)
                {
                    _fertilizerManager = FindObjectOfType<CollectableFetrilizer.FertilizerManager>();
                }
                return _fertilizerManager;
            }
        }
        
        private void Awake()
        {
            _input = new PlayerInputs();
        }
        
        public void OnSceneLoaded()
        {
            _head = FindObjectOfType<HeadController>();
            _meshGenerator = FindObjectOfType<MeshGenerator>();
            _isReloadingScene = false;
        }

        private void OnEnable()
        {
            _input.Enable();
            _input.PlayerInput.MoveCreeper.performed += OnMove;
            _input.PlayerInput.Reset.performed += OnReset;
            _input.PlayerInput.Confirm.performed += OnConfirm;

            _input.PlayerInput.MoveCreeper.canceled += OnCancelMove;
        }

        private void OnDisable()
        {
            _input.Disable();
            _input.PlayerInput.MoveCreeper.performed -= OnMove;
            _input.PlayerInput.Reset.performed -= OnReset;
            _input.PlayerInput.Confirm.performed -= OnConfirm;

            _input.PlayerInput.MoveCreeper.canceled -= OnCancelMove;
        }

        private void OnConfirm(InputAction.CallbackContext obj)
        {
            if (FertilizerManager == null) return;
            
            if (FertilizerManager.IsDelivered)
            {
                Game.Instance.OnLoadFirstLevel();
            }
                
        }

        public void OnMove(InputAction.CallbackContext _directionCallback)
        {
            if (_isInputFrozen) return;
            
            var direction = _directionCallback.ReadValue<Vector2>();
            if (direction.magnitude > 1f)
            {
                direction.Normalize();
            }
            _head.InputDirection = direction;
            _meshGenerator.InputDirection = direction;
        }

        public void OnCancelMove(InputAction.CallbackContext _directionCallback)
        {
            if (_isInputFrozen) return;

            _head.InputDirection = Vector3.zero;
            _meshGenerator.InputDirection = Vector3.zero;
        }

        public void OnReset(InputAction.CallbackContext _directionCallback)
        {
            if (_isReloadingScene || _isInputFrozen) return;

            _isReloadingScene = true;
            Game.Instance.OnLoadFirstLevel();
        }

        public void FreezeInputs()
        {
            _isInputFrozen = true;
        }

        public void UnfreezeInputs()
        {
            _isInputFrozen = false;
        }
    }
}
