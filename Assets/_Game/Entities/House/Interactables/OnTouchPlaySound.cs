using UnityEngine;

namespace Interactables
{
    public class OnTouchPlaySound : MonoBehaviour
    {
        public string sfxId;
        private void OnCollisionEnter(Collision other)
        {
            if (!Utils.Helper.IsLayerPlayerPhysicsCollider(other.gameObject.layer)) return;
            
            // TODO: Play sfx here and use sfxId
        }
    }
}
