using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Roots
{
    public class Game : MonoBehaviour
    {
        private static Game _instance;
        public static Game Instance => _instance;
        public Creeper.InputController inputController;
        public bool isFirstRound = true;

        public GameUI.UI ui;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (!isFirstRound) return;
            
            OnLoadTitle();
        }

        public void OnLoadTitle()
        {
            SceneManager.LoadSceneAsync(1);
        }

        public void OnLoadFirstLevel()
        {
            StartCoroutine(ui.FadeOut(() => SceneManager.LoadSceneAsync(2)));
        }
        
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
 
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(ui.FadeIn(null));
            inputController.OnSceneLoaded();
        }
    }
}
    
