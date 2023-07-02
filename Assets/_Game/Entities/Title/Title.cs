using System.Collections;
using UnityEngine;

namespace Roots
{
    
    public class Title : MonoBehaviour
    {
        public Animator[] animators;
        private Coroutine coroutine;
        public void OnConfirm()
        {
            if (coroutine != null) return;
            
            foreach (var animator in animators)
            {
                animator.enabled = true;
            }

            coroutine = StartCoroutine(LoadLevelAfterAnimation());
        }
        
        private IEnumerator LoadLevelAfterAnimation()
        {
            yield return new WaitForSeconds(2.0f);
            Game.Instance.OnLoadFirstLevel();
        }
    }
}
