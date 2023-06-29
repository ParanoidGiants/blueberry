using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public Image background;
    
    private float animateTimer = 2f;
    public IEnumerator FadeOut(Action callback)
    {
        var time = 0f;
        Color color;
        while (time < animateTimer)
        {
            time += Time.deltaTime;
            var alpha = Mathf.Lerp(0, 1, time / animateTimer);
            color = background.color;
            color.a = alpha;
            background.color = color;
            yield return null;
        }
        color = background.color;
        color.a = 1f;
        background.color = color;
        callback();
    }

    public IEnumerator FadeIn(Action action)
    {
        if (action != null) action();
        
        var time = 0f;
        Color color;
        while (time < animateTimer)
        {
            time += Time.deltaTime;
            var alpha = Mathf.Lerp(1, 0, time / animateTimer);
            color = background.color;
            color.a = alpha;
            background.color = color;
            yield return null;
        }
        color = background.color;
        color.a = 0f;
        background.color = color;
    }
}
