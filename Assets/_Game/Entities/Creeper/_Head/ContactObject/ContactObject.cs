using System;
using UnityEngine;

namespace Creeper
{
    [Serializable]
    public class ContactObject
    {
        [SerializeField] private int _gameObjectId;
        [SerializeField] private Vector3 _normal;
        
        public ContactObject(int gameObjectId, Vector3 normal)
        {
            this._gameObjectId = gameObjectId;
            this._normal = normal;
        }
        
        public bool Equals(ContactObject other)
        {
            return _gameObjectId == other._gameObjectId 
                   && Utils.Helper.AreDirectionsConsideredEqual(_normal, other._normal);
        }
    }
}