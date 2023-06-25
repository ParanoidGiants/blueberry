using System.Collections;
using Creeper;
using UnityEngine;
using DG.Tweening;
using RootMath;

public class Fertilizer : MonoBehaviour
{
    private Renderer _renderer;
    private Collider _collider;
    private bool _isCollected;
    private bool _isDelivered = false;
    public bool IsCollected => _isCollected;
    public bool IsDelivered => _isDelivered;

    private VineController _vine;
    [SerializeField] private float _animateTimer = 1f;
    private Vector3 _originalScale;
    private float _valueToTween = -5f;
    private float _valueFromTween = 5f;

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
    private void AnimateCollect()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(transform.localScale * 2f, 0.01f).SetEase(Ease.InCirc));
        sequence.Append(transform.DOScale(Vector3.zero, _animateTimer * 0.8f).SetEase(Ease.OutCirc));
        sequence.Join(transform.DOMove(_vine.GetTipPosition(), _animateTimer));
        
        _valueToTween = -5f;
        DOTween.To(
                () => _valueToTween,
                x => _valueToTween = x,
                5f, 
                _animateTimer
            )
            .OnUpdate(() => _renderer.material.SetFloat("_FresnelPower", _valueToTween));
    }

    public void OnDeliver(Vector3 deliveryPosition)
    {
        if (_isDelivered) return;
        
        _isDelivered = true;
        
        AnimateDelivery(deliveryPosition);
    }
    

    public void AnimateDelivery(Vector3 endPosition)
    {
        transform.position = _vine.GetTipPosition();
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(transform.DOScale(_originalScale, _animateTimer)); // Scale to original size over 1 second
        mySequence.Append(transform.DOMove(endPosition, _animateTimer)); // Move to end position over 1 second
        mySequence.Append(transform.DOScale(0, _animateTimer)); // Scale back to zero over 1 second
    }
}
