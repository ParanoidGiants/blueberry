using UnityEngine;
using DG.Tweening;

public class Fertilizer : MonoBehaviour
{
    private Renderer _renderer;
    private Collider _collider;
    private bool _isCollected;
    private bool _isCollecting;
    private bool _isDelivered;
    public bool IsCollected => _isCollected;
    public bool IsDelivered => _isDelivered;

    private VineController _vine;
    [SerializeField] private float _animateTimer = 1f;
    private Vector3 _originalScale;

    private void Start()
    {
        _renderer = GetComponentInChildren<Renderer>();
        _collider = GetComponent<Collider>();
        _vine = FindObjectOfType<VineController>();
        _originalScale = transform.localScale;
    }

    private void OnTriggerEnter(Collider other)
    {
        FertilizerManager manager = other.GetComponent<FertilizerManager>();
        if (manager == null) return;
        
        _isCollected = true;
        _collider.enabled = false;
        manager.OnCollectFertilizer(this);
        AnimateCollect();
    }

    private float time;
    private Vector3 startPosition;
    private static readonly int FresnelPower = Shader.PropertyToID("_FresnelPower");

    private void Update()
    {
        if (!_isCollecting) return;

        time += Time.deltaTime;
        transform.position = Vector3.Lerp(startPosition, _vine.GetTipPosition(), time /_animateTimer);
        
        float value = Mathf.Lerp(-5f, 5f, time / _animateTimer);
        _renderer.material.SetFloat(FresnelPower, value);
            
            
        if (time >= _animateTimer)
        {
            _isCollecting = false;
            _isCollected = true;
            transform.position = _vine.GetTipPosition();
            _renderer.material.SetFloat(FresnelPower, 5f);
            _vine.AddFlower();
        }
    }


    private void AnimateCollect()
    {
        startPosition = transform.position;
        time = 0f;
        _isCollecting = true;
        var sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(transform.localScale * 2f, 0.01f).SetEase(Ease.InCirc));
        sequence.Append(transform.DOScale(Vector3.zero, _animateTimer - 0.01f).SetEase(Ease.OutCirc));
    }

    public void OnDeliver(Vector3 deliveryPosition)
    {
        if (_isDelivered) return;
        AnimateDelivery(deliveryPosition);
    }
    

    public void AnimateDelivery(Vector3 endPosition)
    {
        transform.position = _vine.GetTipPosition();
        var sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(_originalScale, _animateTimer)); // Scale to original size over 1 second
        sequence.Append(transform.DOMove(endPosition, _animateTimer)); // Move to end position over 1 second
        sequence.Append(transform.DOScale(0, _animateTimer)); // Scale back to zero over 1 second
        sequence.OnComplete(() => _isDelivered = true);
    }
}
