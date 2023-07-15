using Creeper;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameUI
{
    public class WindowConversationUI : MonoBehaviour
    {
        private TextMeshProUGUI _textMesh;
        private Image _background;
        private RectTransform _rectTransform;
        private bool _isConversationActive;
        private Vector3 _currentTargetPosition;
        private Camera _camera;
        private Transform _player;
        
        [Header("Settings")]
        [SerializeField] private float _minDistance = 2f;
        [SerializeField] private float _maxDistance = 10f;
        [SerializeField] private float _minScale = 0.5f;
        [SerializeField] private float _maxScale = 1f;

        private void Start()
        {
            _textMesh = GetComponentInChildren<TextMeshProUGUI>();
            _rectTransform = GetComponent<RectTransform>();
            _background = GetComponent<Image>();
            DisableConversation();
            _camera = Camera.main;
            _player = FindObjectOfType<HeadController>().transform;
        }

        private void Update()
        {
            _rectTransform.position = (Vector2) _camera.WorldToScreenPoint(_currentTargetPosition);

            var distanceToPlayer = Mathf.Clamp((_currentTargetPosition - _player.position).magnitude, _minDistance, _maxDistance);
            var newScale = Utils.Helper.Remap(distanceToPlayer, _minDistance, _maxDistance, _maxScale, _minScale);
            _rectTransform.localScale = newScale * Vector3.one;
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
}
