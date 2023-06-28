using System;
using System.Linq;
using System.Collections.Generic;
using RootMath;
using UnityEngine;

namespace Creeper
{
    [Serializable]
    public class ContactObjectManager
    {
        public Dictionary<int, List<Vector3>> contactObjects;
        public Vector3 normal;
        
        public ContactObjectManager()
        {
            contactObjects = new Dictionary<int, List<Vector3>>();
        }
        
        public void UpdateGround()
        {
            if (contactObjects.Count == 0)
            {
                return;
            }
            normal = Vector3.zero;
            foreach (var contactObject in contactObjects.Values)
            {
                foreach (var contactNormal in contactObject)
                {
                    normal += contactNormal;
                }
            }
            normal.Normalize();
        }
        
        public bool TryAddContactObject(int collisionInstanceId, Vector3 normal)
        {
            if (!contactObjects.ContainsKey(collisionInstanceId))
            {
                contactObjects.Add(collisionInstanceId, new List<Vector3>());
                contactObjects[collisionInstanceId].Add(normal);
                return true;
            }
            if (!contactObjects[collisionInstanceId].Contains(normal))
            {
                contactObjects[collisionInstanceId].Add(normal);
                return true;
            }
            
            return false;
        }

        public bool TryAddNormals(Collision collision)
        {
            var collisionInstanceId = collision.gameObject.GetInstanceID();
            if (contactObjects.ContainsKey(collisionInstanceId) && contactObjects[collisionInstanceId].Count == collision.contactCount)
            {
                var oldList = new List<Vector3>(contactObjects[collisionInstanceId]);
                contactObjects[collisionInstanceId].Clear();
                
                bool isCollisionNew = false;
                for (int i = 0; i < collision.contactCount; i++)
                {
                    if (!oldList.Any(x => RMath.AreDirectionsConsideredEqual(x, collision.GetContact(i).normal)))
                    {
                        isCollisionNew = true;
                    }
                    contactObjects[collisionInstanceId].Add(collision.GetContact(i).normal);
                }
                return isCollisionNew;
            }
            else if (!contactObjects.ContainsKey(collisionInstanceId))
            {
                contactObjects.Add(collisionInstanceId, new List<Vector3>());
                for (int i = 0; i < collision.contactCount; i++)
                {
                    contactObjects[collisionInstanceId].Add(collision.GetContact(i).normal);
                }

                return true;
            }
            else
            {
                contactObjects[collisionInstanceId].Clear();
                for (int i = 0; i < collision.contactCount; i++)
                {
                    contactObjects[collisionInstanceId].Add(collision.GetContact(i).normal);
                }

                return true;
            }
        }
        
        public void RemoveContactObjects(int collisionInstanceId)
        {
            contactObjects.Remove(collisionInstanceId);
        }
    }
}