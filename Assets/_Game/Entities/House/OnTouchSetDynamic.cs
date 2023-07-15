using UnityEngine;

namespace Interactables
{
       public class OnTouchSetDynamic : MonoBehaviour
       {
              private Rigidbody targetRigidbody;

              private void Awake()
              {
                     targetRigidbody = GetComponent<Rigidbody>();
                     targetRigidbody.isKinematic = true;
              }

              private void OnCollisionEnter(Collision other)
              {
                     if (!Utils.Helper.IsLayerPlayerPhysicsCollider(other.gameObject.layer)) return;
                     
                     targetRigidbody = GetComponent<Rigidbody>();
                     targetRigidbody.isKinematic = false;
              }
       }
}
