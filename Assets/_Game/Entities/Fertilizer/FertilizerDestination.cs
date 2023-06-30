using System.Collections;
using UnityEngine;

public class FertilizerDestination : MonoBehaviour
{
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private IEnumerator OnTriggerEnter(Collider other)
    {
        var manager = other.GetComponent<FertilizerManager>();
        if (manager == null) yield break;

        manager.InitDeliverFertilizer();
        yield return new WaitUntil(() => !manager.IsDelivering);
        
        if (!manager.IsDelivered) yield break;

        _animator.enabled = true;
    }
}
