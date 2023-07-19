using UnityEngine;

namespace Interactables
{
       public class OnTouchSetDynamic : MonoBehaviour
       {
              private Rigidbody targetRigidbody;
              public string sfxId;

              private void Awake()
              {
                     targetRigidbody = GetComponent<Rigidbody>();
                     targetRigidbody.isKinematic = true;
              }

              private void OnCollisionEnter(Collision other)
              {
                     if (!Utils.Helper.IsLayerPlayerPhysicsCollider(other.gameObject.layer)) return;
                     
                     // TODO: Play sfx here and use sfxId
                     targetRigidbody = GetComponent<Rigidbody>();
                     targetRigidbody.isKinematic = false;
              }
       }
}
