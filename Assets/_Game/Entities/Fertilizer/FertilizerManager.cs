using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FertilizerManager : MonoBehaviour
{
    private List<Fertilizer> _fertilizers;
    private int _totalAmount = 0;
    private int _count = 0;
    private FertilizerText _fertilizerText;
    public int Count => _count;

    private void Awake()
    {
        _fertilizers = FindObjectsOfType<Fertilizer>().ToList();
        _fertilizerText = FindObjectOfType<FertilizerText>();
        _totalAmount = _fertilizers.Count;
        _fertilizerText.UpdateText(_count, _totalAmount);
    }
    
    public void OnCollectFertilizer(Fertilizer fertilizer)
    {   
        _fertilizers.Remove(fertilizer);
        _count++;
        _fertilizerText.UpdateText(_count, _totalAmount);
    }
}
