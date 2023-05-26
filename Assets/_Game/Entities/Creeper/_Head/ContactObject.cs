using System;
using RootMath;
using UnityEngine;

namespace Creeper
{
    [Serializable]
    public class ContactObject
    {
        public int gameObjectId;
        public Vector3 normal;
        
        public ContactObject(int gameObjectId, Vector3 normal)
        {
            this.gameObjectId = gameObjectId;
            this.normal = normal;
        }
        
        public bool Equals(ContactObject other)
        {
            return gameObjectId == other.gameObjectId 
                   && RMath.AreDirectionsConsideredEqual(normal, other.normal);
        }
    }
}