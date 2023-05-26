using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        CollectableManager manager = other.GetComponent<CollectableManager>();
        if (manager != null)
        {
            manager.CollectableCollected();
            Debug.Log("Amount of Collectables: " + manager.collectableCounter);
            Destroy(gameObject);
        }
    }
}
