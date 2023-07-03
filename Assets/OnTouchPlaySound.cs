using RootMath;
using UnityEngine;

public class OnTouchPlaySound : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if (!RMath.IsLayerPlayerPhysicsCollider(other.gameObject.layer)) return;
        
        // TODO: Play sound here
        Debug.Log("Play sound here");
    }
}
