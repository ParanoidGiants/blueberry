using TMPro;
using UnityEngine;

namespace GameUI
{
    public class FertilizerText : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        private Animator _animator;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            _animator = GetComponent<Animator>();
        }

        public void UpdateText(int currentCount, int totalCount)
        {
            _text.text = $"{currentCount.ToString()} / {totalCount.ToString()}";
            _animator.SetTrigger("Pulse");
        }
    }
}
