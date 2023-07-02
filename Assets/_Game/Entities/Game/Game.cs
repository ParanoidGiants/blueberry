using UnityEngine;
using UnityEngine.SceneManagement;

namespace Roots
{
    public class Game : MonoBehaviour
    {
        private static Game _instance;
        public static Game Instance => _instance;
        public bool IsOnTitle { get; private set; }
        public bool IsLevelDone => _fertilizerManager != null && _fertilizerManager.IsDelivered;
        
        public UI ui;
        private FertilizerManager _fertilizerManager;
        
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            OnLoadTitle();
        }

        public void OnLoadTitle()
        {
            SceneManager.LoadSceneAsync(1);
            IsOnTitle = true;
        }

        public void OnLoadFirstLevel()
        {
            IsOnTitle = false;
            StartCoroutine(
                ui.FadeOut(
                    () => SceneManager.LoadSceneAsync(2)
                )
            );
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
            _fertilizerManager = FindObjectOfType<FertilizerManager>();
        }
    }
}
    
