using System.Collections;
using UnityEngine;

public class Fertilizer : MonoBehaviour
{
    private Renderer _renderer;
    private Collider _collider;
    private bool _isCollected;
    private bool _isDelivered;
    public bool IsCollected => _isCollected;
    public bool IsDelivered => _isDelivered;

    private void Start()
    {
        _renderer = GetComponentInChildren<Renderer>();
        _collider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        FertilizerManager manager = other.GetComponent<FertilizerManager>();
        if (manager == null) return;
        
        manager.OnCollectFertilizer(this);
        SetCollected();
    }

    private void SetCollected()
    {
        _isCollected = true;
        _collider.enabled = false;
        _renderer.gameObject.SetActive(false);
    }

    public void OnDeliver(Vector3 startposition, Vector3 deliveryPosition)
    {
        if (_isDelivered) return;
        
        _isDelivered = true;
        StartCoroutine(Deliver(startposition, deliveryPosition));
    }


    private float animateFor = 0.5f;
    private IEnumerator Deliver(Vector3 startPosition, Vector3 deliveryPosition)
    {
        _renderer.gameObject.SetActive(true);
        
        var time = 0f;
        while (time < animateFor)
        {
            transform.position = Vector3.Lerp(startPosition, deliveryPosition, time / animateFor);
            time += Time.deltaTime;
            yield return null;
        }
        _renderer.gameObject.SetActive(false);
    }
}
