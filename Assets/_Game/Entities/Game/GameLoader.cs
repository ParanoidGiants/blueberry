using UnityEngine;
using UnityEngine.SceneManagement;

namespace Roots
{
    public class GameLoader : MonoBehaviour
    {
        private static GameLoader _instance;
        public static GameLoader Instance => _instance;
        
        
        [Header("References")]
        public Creeper.InputController inputController;
        public GameUI.Fader fader;
        
        private bool _showIntro = true;
        public bool ShowIntro => _showIntro;
        public void WatchIntro()
        {
            _showIntro = false;
        }
        
        private bool _isOnTitle;
        public bool IsOnTitle => _isOnTitle;

#if UNITY_EDITOR
        [Space(10)]
        [Header("SET FALSE WHEN YOU NOT START RUNNING IN ENTRY SCENE")]
        public bool initialSceneIsEntryScene;
#endif

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
#if UNITY_EDITOR
            if (!initialSceneIsEntryScene) return;
#endif
            
            OnLoadTitle();
        }

        public void OnLoadTitle()
        {
            SceneManager.LoadSceneAsync(1);
        }

        public void OnLoadFirstLevel()
        {
            StartCoroutine(fader.FadeOut(() => SceneManager.LoadSceneAsync(2)));
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
            _isOnTitle = scene.name == "_Title";
            StartCoroutine(fader.FadeIn(null));
            inputController.OnSceneLoaded();
        }
    }
}
    
