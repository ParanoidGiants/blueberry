using System.Collections;
using UnityEngine;
using DG.Tweening;

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
        public bool IsCollected => _isCollected;
        private bool _isCollecting;

        private bool _isDelivered;
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
            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOScale(transform.localScale * 2f, 0.01f).SetEase(Ease.InCirc));
            sequence.Append(transform.DOScale(Vector3.zero, ANIMATE_TIMER - 0.01f).SetEase(Ease.OutCirc));
        }

        public IEnumerator OnAnimateDelivery(Vector3 deliveryPosition)
        {
            transform.position = _meshGenerator.GetTipPosition();
            var animateTime = 0f;

            while (animateTime < ANIMATE_TIMER)
            {
                animateTime += Time.deltaTime;
                transform.localScale = Vector3.Lerp(Vector3.zero, _originalScale, animateTime / ANIMATE_TIMER);
                yield return null;
            } 

            animateTime = 0f;
            while (animateTime < ANIMATE_TIMER)
            {
                animateTime += Time.deltaTime;
                transform.position = Vector3.Lerp(_meshGenerator.GetTipPosition(), deliveryPosition, animateTime / ANIMATE_TIMER);
                yield return null;
            }
            
            animateTime = 0f;
            while (animateTime < ANIMATE_TIMER)
            {
                animateTime += Time.deltaTime;
                transform.localScale = Vector3.Lerp(_originalScale, Vector3.zero, animateTime / ANIMATE_TIMER);
                yield return null;
            }
            _isDelivered = true;
        }
    }
}