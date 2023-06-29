using System.Collections;
using Creeper;
using UnityEngine;

public class FertilizerDestination : MonoBehaviour
{
    public Animator flowerAnimator;
    public CameraController camera;
    private IEnumerator OnTriggerEnter(Collider other)
    {
        var manager = other.GetComponent<FertilizerManager>();
        if (manager == null) yield break;

        manager.InitDeliverFertilizer();
        yield return new WaitUntil(() => !manager.IsDelivering);
        
        if (!manager.IsDelivered) yield break;
        
        StartCoroutine(camera.MoveCameraToFlower(new Vector3(4.5f, 54f, -0.2f)));

        yield return new WaitUntil(() => camera.IsFlowerFocused);
        flowerAnimator.enabled = true;
    }
}
