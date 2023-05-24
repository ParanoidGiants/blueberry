using System;
using System.Collections.Generic;
using System.Linq;
using RootMath;
using UnityEngine;

namespace Creeper
{
    [Serializable]
    public class ContactObjectManager
    {
        public List<ContactObject> contactObjects = new List<ContactObject>();
        public Vector3 normal;
        
        public void UpdateGround()
        {
            if (contactObjects.Count == 0)
            {
                Debug.Log("No new ground direction found");
                return;
            }
            normal = Vector3.zero;
            foreach (var contactObject in contactObjects)
            {
                normal += contactObject.normal;
            }
            normal.Normalize();
        }
        
        public void TryAddContactObject(int gameObjectId, Vector3 normal)
        {
            foreach (var contactObject in contactObjects)
            {
                if (contactObject.gameObjectId == gameObjectId
                    && RMath.AreDirectionsConsideredEqual(contactObject.normal, normal)
                   )
                {
                    return;
                }
            }

            contactObjects.Add(new ContactObject(gameObjectId, normal));
            UpdateGround();
        }

        public void TryAddNormals(Collision collision)
        {
            var collisionInstanceId = collision.gameObject.GetInstanceID();
            for (int i = 0; i < collision.contactCount; i++)
            {
                TryAddContactObject(collisionInstanceId, collision.GetContact(i).normal);
            }
            UpdateGround();
        }
        
        public void RemoveContactObjects(int gameObjectId)
        {
            contactObjects.RemoveAll(co => co.gameObjectId == gameObjectId);
        }

        public void RemoveContactObjects(Collision collision)
        {
            contactObjects.RemoveAll(x => x.gameObjectId == collision.gameObject.GetInstanceID());
        }
    }
}