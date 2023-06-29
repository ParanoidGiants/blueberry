using System.Collections;
using UnityEngine;

public class FertilizerDestination : MonoBehaviour
{
    public Animator flowerAnimator;
    private IEnumerator OnTriggerEnter(Collider other)
    {
        var manager = other.GetComponent<FertilizerManager>();
        if (manager == null) yield break;

        manager.InitDeliverFertilizer();
        yield return new WaitUntil(() => !manager.IsDelivering);
        if (manager.IsDelivered)
        {
            flowerAnimator.enabled = true;
        }
    }
}
