using UnityEngine;
using DG.Tweening;
using DarkTonic.MasterAudio;

namespace CollectableFetrilizer
{
    public class Fertilizer : MonoBehaviour
    {
        private const float ANIMATE_TIMER = 1f;
        private readonly int _fresnelPower = Shader.PropertyToID("_FresnelPower");
        
        private Renderer _renderer;
        private Collider _collider;
        private Creeper.MeshGenerator _meshGenerator;
        private Vector3 _originalScale;
        private float _time;
        private Vector3 _startPosition;
        
        private bool _isCollected;
        private bool _isCollecting;
        private bool _isDelivered;
        public bool IsCollected => _isCollected;
        public bool IsDelivered => _isDelivered;

        private void Start()
        {
            _renderer = GetComponentInChildren<Renderer>();
            _collider = GetComponent<Collider>();
            _meshGenerator = FindObjectOfType<Creeper.MeshGenerator>();
            _originalScale = transform.localScale;
        }

        private void OnTriggerEnter(Collider other)
        {
            var manager = other.GetComponent<FertilizerManager>();
            if (manager == null) return;
            
            _isCollected = true;
            _collider.enabled = false;
            manager.OnCollectFertilizer(this);
            AnimateCollect();
        }

        private void Update()
        {
            if (!_isCollecting) return;

            _time += Time.deltaTime;
            transform.position = Vector3.Lerp(_startPosition, _meshGenerator.GetTipPosition(), _time /ANIMATE_TIMER);
            
            float value = Mathf.Lerp(-5f, 5f, _time / ANIMATE_TIMER);
            _renderer.material.SetFloat(_fresnelPower, value);
                
                
            if (_time < ANIMATE_TIMER) return;
            
            _isCollecting = false;
            _isCollected = true;
            transform.position = _meshGenerator.GetTipPosition();
            _renderer.material.SetFloat(_fresnelPower, 5f);
            _meshGenerator.AddFlower();
        }


        private void AnimateCollect()
        {
            _startPosition = transform.position;
            _time = 0f;
            _isCollecting = true;
            MasterAudio.PlaySound3DAtTransformAndForget("Collectable", transform);
            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOScale(transform.localScale * 2f, 0.01f).SetEase(Ease.InCirc));
            sequence.Append(transform.DOScale(Vector3.zero, ANIMATE_TIMER - 0.01f).SetEase(Ease.OutCirc));
        }

        public void OnDeliver(Vector3 deliveryPosition)
        {
            if (_isDelivered) return;
            AnimateDelivery(deliveryPosition);
        }
        

        public void AnimateDelivery(Vector3 endPosition)
        {
            transform.position = _meshGenerator.GetTipPosition();
            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOScale(_originalScale, ANIMATE_TIMER)); // Scale to original size over 1 second
            sequence.Append(transform.DOMove(endPosition, ANIMATE_TIMER)); // Move to end position over 1 second
            sequence.Append(transform.DOScale(0, ANIMATE_TIMER)); // Scale back to zero over 1 second
            sequence.OnComplete(() => _isDelivered = true);
        }
    }
}