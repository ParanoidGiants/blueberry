using System.Collections;
using UnityEngine;

namespace GameUI
{
    public class Title : MonoBehaviour
    {
        public Animator animator;
        private bool _isAnimating; 
        private static readonly int StartGame = Animator.StringToHash("StartGame");

        public void OnConfirm()
        {
            if (_isAnimating) return;
            _isAnimating = true;
            
            animator.SetTrigger("StartGame");
        }
        
        public void OnLoadLevelAnimationEvent()
        {
            Roots.GameLoader.Instance.OnLoadFirstLevel();
        }
    }
}
