using System;
using TMPro;
using UnityEngine;

public class FertilizerText : MonoBehaviour
{
    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    public void UpdateText(int currentCount, int totalCount)
    {
        _text.text = $"<b>Fertilizer</b> {currentCount.ToString()} / {totalCount.ToString()}";
    }
}
