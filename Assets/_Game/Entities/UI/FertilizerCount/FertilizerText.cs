using TMPro;
using UnityEngine;

namespace GameUI
{
    public class FertilizerText : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        private Animator _animator;
        private static readonly int Pulse = Animator.StringToHash("Pulse");

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            _animator = GetComponent<Animator>();
        }

        public void UpdateText(int currentCount, int totalCount, bool pulse)
        {
            if (pulse)
            {
                _animator.SetTrigger(Pulse);
            }
            _text.text = $"{currentCount.ToString()} / {totalCount.ToString()}";
        }
    }
}
