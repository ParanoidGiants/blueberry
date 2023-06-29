using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FertilizerManager : MonoBehaviour
{
    public VineController vine;
    private List<Fertilizer> _fertilizers;
    private int _totalAmount = 0;
    private int _count = 0;
    private FertilizerText _fertilizerText;
    private FertilizerDestination _destination;
    private bool _isDelivering = false;
    public bool IsDelivering => _isDelivering;
    private bool _isDelivered = false;
    public bool IsDelivered => _isDelivered;
    public int Count => _count;

    private void Awake()
    {
        _fertilizers = FindObjectsOfType<Fertilizer>().ToList();
        _fertilizerText = FindObjectOfType<FertilizerText>();
        _destination = FindObjectOfType<FertilizerDestination>();
        _totalAmount = _fertilizers.Count;
    }

    private void Start()
    {
        UpdateUI();
    }

    public void OnCollectFertilizer(Fertilizer fertilizer)
    {
        _count++;
        UpdateUI();
    }

    public void InitDeliverFertilizer()
    {
        if (_isDelivering) return;
        StartCoroutine(DeliverFertilizer());
    }

    private IEnumerator DeliverFertilizer()
    {
        _isDelivering = true;
        var collectedFertilizers = _fertilizers.Where(x => x.IsCollected && !x.IsDelivered).ToList();

        foreach (var collectedFertilizer in collectedFertilizers)
        {
            collectedFertilizer.OnDeliver(_destination.transform.position);
            _count--;
            _totalAmount--;
            UpdateUI();
            yield return new WaitForSeconds(0.2f);
        }
        yield return new WaitUntil(() => AreAllFertilizersDelivered(collectedFertilizers));
        
        _isDelivered = _totalAmount == 0;
        _isDelivering = false;
    }

    private bool AreAllFertilizersDelivered(List<Fertilizer> collected)
    {
        foreach (var fertilizer in collected)
        {
            if (!fertilizer.IsDelivered) return false;
        }
        return true;
    }

    private void UpdateUI()
    {
        _fertilizerText.UpdateText(_count, _totalAmount);
    }
}
