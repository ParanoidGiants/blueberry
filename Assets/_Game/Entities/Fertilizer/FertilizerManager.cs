using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FertilizerManager : MonoBehaviour
{
    private List<Fertilizer> _fertilizers;
    private int _totalAmount = 0;
    private int _count = 0;
    private FertilizerText _fertilizerText;
    private FertilizerDestination _destination;
    private bool isDelivering = false;
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
        StartCoroutine(DeliverFertilizer());
    }

    private IEnumerator DeliverFertilizer()
    {
        isDelivering = true;
        var collectedFertilizers = _fertilizers.Where(x => x.IsCollected && !x.IsDelivered);

        foreach (var collectedFertilizer in collectedFertilizers)
        {
            collectedFertilizer.OnDeliver(_destination.transform.position);
            _count--;
            _totalAmount--;
            UpdateUI();
            yield return new WaitForSeconds(0.2f);
        }
        isDelivering = false;
    }
    
    private void UpdateUI()
    {
        _fertilizerText.UpdateText(_count, _totalAmount);
    }
}
