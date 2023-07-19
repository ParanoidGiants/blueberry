using UnityEngine;

namespace Interactables
{
    public class OnTouchBreakJoint : MonoBehaviour
    {
        public string sfxId;
        private void OnCollisionEnter(Collision other)
        {
            if (!Utils.Helper.IsLayerPlayerPhysicsCollider(other.gameObject.layer)) return;
            
            // TODO: Play sfx here and use sfxId
            var springJoint = GetComponent<SpringJoint>();
            springJoint.breakForce = 0;
            Destroy(this);
        }
    }
}