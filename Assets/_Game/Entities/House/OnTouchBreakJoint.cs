using RootMath;
using UnityEngine;

public class OnTouchBreakJoint : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if (!RMath.IsLayerPlayerPhysicsCollider(other.gameObject.layer)) return;
        
        var springJoint = GetComponent<SpringJoint>();
        springJoint.breakForce = 0;
        Destroy(this);
    }
}
