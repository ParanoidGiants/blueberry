using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowConversationUI : MonoBehaviour
{
    private TextMeshProUGUI _textMesh;
    private Image _background;
    private RectTransform _rectTransform;
    private bool _isConversationActive;
    public int pixelOffsetHorizontal = 0;
    public int pixelOffsetVertical = 0;
    private Vector3 _currentTargetPosition;
    private Camera _camera;

    private void Start()
    {
        _textMesh = GetComponentInChildren<TextMeshProUGUI>();
        _rectTransform = GetComponent<RectTransform>();
        _background = GetComponent<Image>();
        DisableConversation();
        _camera = Camera.main;
    }

    private void Update()
    {
        var sizeDelta = _rectTransform.sizeDelta;
        var minX = sizeDelta.x / 2 + pixelOffsetHorizontal;
        var maxX = _camera.scaledPixelWidth - sizeDelta.x / 2 - pixelOffsetHorizontal;
        var minY = sizeDelta.y / 2 + pixelOffsetVertical;
        var maxY = _camera.scaledPixelHeight - sizeDelta.y / 2 - pixelOffsetVertical;
            
        var newPosition = _camera.WorldToScreenPoint(_currentTargetPosition);
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y, -maxY, -minY);
        _rectTransform.anchoredPosition = newPosition;
    }

    public void SetText(string conversationSnippet, Vector3 windowCenter)
    {
        _background.enabled = true;
        _textMesh.enabled = true;
        _currentTargetPosition = windowCenter;
        _textMesh.text = conversationSnippet;
    }

    public void DisableConversation()
    {
        _background.enabled = false;
        _textMesh.enabled = false;
    }
}
