using Creeper;
using Roots;
using UnityEngine;
using UnityEngine.Playables;

namespace Level
{
    public class StartLevel : MonoBehaviour
    {
        [SerializeField] private PlayableDirector _startLevelDirector;
        private InputController _inputController;

        private void Start()
        {
            if (!Game.Instance.isFirstRound) return;
            Game.Instance.isFirstRound = false;
            
            _inputController = Game.Instance.inputController;
            _inputController.FreezeInputs();
            _startLevelDirector.Play();
        }

        public void OnStartLevel()
        {
            _inputController.UnfreezeInputs();
        }
    }
}