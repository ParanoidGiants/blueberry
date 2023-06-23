using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FertilizerManager : MonoBehaviour
{
    private List<Fertilizer> _fertilizers;
    private int _totalAmount = 0;
    private int _count = 0;
    public int Count => _count;

    private void Awake()
    {
        _fertilizers = FindObjectsOfType<Fertilizer>().ToList();
        _totalAmount = _fertilizers.Count;
    }
    
    public void OnCollectFertilizer(Fertilizer fertilizer)
    {   
        _fertilizers.Remove(fertilizer);
        _count++;
        Debug.Log($"Amount of Collectables: {_count}");
    }
}
