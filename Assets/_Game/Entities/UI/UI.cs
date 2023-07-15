using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GameUI
{
    public class UI : MonoBehaviour
    {
        private const float ANIMATE_FOR_SECONDS = 2f; 
        [SerializeField] private Image _background;
        
        public IEnumerator FadeOut(Action callback)
        {
            var time = 0f;
            Color color;
            while (time < ANIMATE_FOR_SECONDS)
            {
                time += Time.deltaTime;
                var alpha = Mathf.Lerp(0, 1, time / ANIMATE_FOR_SECONDS);
                color = _background.color;
                color.a = alpha;
                _background.color = color;
                yield return null;
            }
            color = _background.color;
            color.a = 1f;
            _background.color = color;
            callback();
        }

        public IEnumerator FadeIn(Action action)
        {
            if (action != null) action();
            
            var time = 0f;
            Color color;
            while (time < ANIMATE_FOR_SECONDS)
            {
                time += Time.deltaTime;
                var alpha = Mathf.Lerp(1, 0, time / ANIMATE_FOR_SECONDS);
                color = _background.color;
                color.a = alpha;
                _background.color = color;
                yield return null;
            }
            color = _background.color;
            color.a = 0f;
            _background.color = color;
        }
    }
}