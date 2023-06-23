using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fertilizer : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        FertilizerManager manager = other.GetComponent<FertilizerManager>();
        if (manager != null)
        {
            manager.OnCollectFertilizer(this);
            Destroy(gameObject);
        }
    }
}
