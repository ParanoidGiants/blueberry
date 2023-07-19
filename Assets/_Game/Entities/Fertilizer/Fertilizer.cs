using System.Collections;
using UnityEngine;
using DarkTonic.MasterAudio;

namespace CollectableFetrilizer
{
    public class Fertilizer : MonoBehaviour
    {
        private const float ANIMATE_TIMER = 0.5f;
        private readonly int _fresnelPower = Shader.PropertyToID("_FresnelPower");
        
        private Renderer _renderer;
        private Collider _collider;
        private Creeper.MeshGenerator _meshGenerator;
        private Vector3 _originalScale;
        private Vector3 _startPosition;
        
        private bool _isCollected;
        public bool IsCollected => _isCollected;

        private bool _isDelivered;
        public bool IsDelivered => _isDelivered;

        private void Start()
        {
            _renderer = GetComponentInChildren<Renderer>();
            _collider = GetComponent<Collider>();
            _meshGenerator = FindObjectOfType<Creeper.MeshGenerator>();
            _originalScale = transform.localScale;
        }

        private IEnumerator OnTriggerEnter(Collider other)
        {
            var manager = other.GetComponent<FertilizerManager>();
            if (manager == null) yield break;
            
            _isCollected = true;
            _collider.enabled = false;
            manager.OnCollectFertilizer(this);
            yield return StartCoroutine(AnimateCollect());
        }

        private IEnumerator AnimateCollect()
        {
            _startPosition = transform.position;
            MasterAudio.PlaySound3DAtTransformAndForget("Collectable", transform);
            var animateTime = 0f;
            while (animateTime < ANIMATE_TIMER)
            {
                animateTime += Time.deltaTime;
                var animateProgress = animateTime / ANIMATE_TIMER; 
                transform.position = Vector3.Lerp(_startPosition, _meshGenerator.GetTipPosition(), animateProgress);
                
                transform.localScale = Vector3.Lerp(2f * _originalScale, Vector3.zero, animateProgress);
                
                float value = Mathf.Lerp(-5f, 5f, animateTime / ANIMATE_TIMER);
                _renderer.material.SetFloat(_fresnelPower, value);
                yield return null;
            }
            
            transform.position = _meshGenerator.GetTipPosition();
            _renderer.material.SetFloat(_fresnelPower, 5f);
            _meshGenerator.AddFlower();
            
            _isCollected = true;
        }

        public IEnumerator OnAnimateDelivery(Vector3 startPosition, Vector3 deliveryPosition)
        {
            transform.position = startPosition;
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
                transform.localScale = Vector3.Lerp(_originalScale, Vector3.zero, animateTime / ANIMATE_TIMER);
                transform.position = Vector3.Lerp(startPosition, deliveryPosition, animateTime / ANIMATE_TIMER);
                yield return null;
            }
            _isDelivered = true;
        }
    }
}
