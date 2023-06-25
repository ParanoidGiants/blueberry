using UnityEngine;

public class FertilizerDestination : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var manager = other.GetComponent<FertilizerManager>();
        if (manager == null) return;

        manager.InitDeliverFertilizer();
    }
}
