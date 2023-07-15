using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Creeper
{
    [Serializable]
    public class ContactObjectManager
    {
        private Dictionary<int, List<Vector3>> _contactObjects;
        private Vector3 _averageNormal;
        public Vector3 AverageNormal => _averageNormal;
        
        public ContactObjectManager()
        {
            _contactObjects = new Dictionary<int, List<Vector3>>();
        }
        
        public void UpdateGround()
        {
            if (_contactObjects.Count == 0)
            {
                return;
            }
            _averageNormal = Vector3.zero;
            foreach (var contactObject in _contactObjects.Values)
            {
                foreach (var contactNormal in contactObject)
                {
                    _averageNormal += contactNormal;
                }
            }
            AverageNormal.Normalize();
        }
        
        public bool TryAddContactObject(int collisionInstanceId, Vector3 normal)
        {
            if (!_contactObjects.ContainsKey(collisionInstanceId))
            {
                _contactObjects.Add(collisionInstanceId, new List<Vector3>());
                _contactObjects[collisionInstanceId].Add(normal);
                return true;
            }
            if (!_contactObjects[collisionInstanceId].Contains(normal))
            {
                _contactObjects[collisionInstanceId].Add(normal);
                return true;
            }
            
            return false;
        }

        public bool TryAddNormals(Collision collision)
        {
            var collisionInstanceId = collision.gameObject.GetInstanceID();
            if (_contactObjects.ContainsKey(collisionInstanceId) && _contactObjects[collisionInstanceId].Count == collision.contactCount)
            {
                var oldList = new List<Vector3>(_contactObjects[collisionInstanceId]);
                _contactObjects[collisionInstanceId].Clear();
                
                bool isCollisionNew = false;
                for (int i = 0; i < collision.contactCount; i++)
                {
                    if (!oldList.Any(x => Utils.Helper.AreDirectionsConsideredEqual(x, collision.GetContact(i).normal)))
                    {
                        isCollisionNew = true;
                    }
                    _contactObjects[collisionInstanceId].Add(collision.GetContact(i).normal);
                }
                return isCollisionNew;
            }
            else if (!_contactObjects.ContainsKey(collisionInstanceId))
            {
                _contactObjects.Add(collisionInstanceId, new List<Vector3>());
                for (int i = 0; i < collision.contactCount; i++)
                {
                    _contactObjects[collisionInstanceId].Add(collision.GetContact(i).normal);
                }

                return true;
            }
            else
            {
                _contactObjects[collisionInstanceId].Clear();
                for (int i = 0; i < collision.contactCount; i++)
                {
                    _contactObjects[collisionInstanceId].Add(collision.GetContact(i).normal);
                }

                return true;
            }
        }
        
        public void RemoveContactObjects(int collisionInstanceId)
        {
            _contactObjects.Remove(collisionInstanceId);
        }

        public bool HasContactObjects()
        {
            return _contactObjects.Count > 0;
        }
    }
}