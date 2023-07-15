using UnityEngine;

namespace Interactables
{
    public class OnTouchPlaySound : MonoBehaviour
    {
        private void OnCollisionEnter(Collision other)
        {
            if (!Utils.Helper.IsLayerPlayerPhysicsCollider(other.gameObject.layer)) return;
            
            // TODO: Play sound here
            Debug.Log("Play sound here");
        }
    }
}
