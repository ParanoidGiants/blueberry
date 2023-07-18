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
            if (!GameLoader.Instance.ShowIntro) return;
            GameLoader.Instance.WatchIntro();
            
            _inputController = GameLoader.Instance.inputController;
            _inputController.FreezeInputs();
            _startLevelDirector.Play();
        }

        public void OnStartLevel()
        {
            _inputController.UnfreezeInputs();
        }
    }
}