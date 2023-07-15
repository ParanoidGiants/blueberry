using Creeper;
using UnityEngine;

namespace Level
{
    public class Start : MonoBehaviour
    {
        private InputController _inputController;

        private void Awake()
        {
            _inputController = FindObjectOfType<InputController>();
            _inputController.FreezeInputs();
        }

        public void OnStartLevel()
        {
            _inputController.UnfreezeInputs();
        }
    }
}