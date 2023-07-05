using System;
using System.Collections;
using System.Collections.Generic;
using RootMath;
using UnityEngine;

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
              if (!RMath.IsLayerPlayerPhysicsCollider(other.gameObject.layer)) return;
              
              targetRigidbody = GetComponent<Rigidbody>();
              targetRigidbody.isKinematic = false;
       }
}
