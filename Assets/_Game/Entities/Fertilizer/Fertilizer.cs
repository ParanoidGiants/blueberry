using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fertilizer : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        CollectFertilizer manager = other.GetComponent<CollectFertilizer>();
        if (manager != null)
        {
            manager.OnCollectFertilizer();
            Debug.Log("Amount of Collectables: " + manager.collectableCounter);
            Destroy(gameObject);
        }
    }
}
