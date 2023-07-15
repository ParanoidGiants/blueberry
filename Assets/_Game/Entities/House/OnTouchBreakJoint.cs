using UnityEngine;

namespace Interactables
{
    public class OnTouchBreakJoint : MonoBehaviour
    {
        private void OnCollisionEnter(Collision other)
        {
            if (!Utils.Helper.IsLayerPlayerPhysicsCollider(other.gameObject.layer)) return;
            
            var springJoint = GetComponent<SpringJoint>();
            springJoint.breakForce = 0;
            Destroy(this);
        }
    }
}