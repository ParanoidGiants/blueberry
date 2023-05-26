using System;
using UnityEngine;

namespace Creeper
{
    [Serializable]
    public class ContactNormal
    {
        public int GameObjectId;
        public Vector3 Normal;

        public ContactNormal(int _gameObjectId, Vector3 _normal)
        {
            GameObjectId = _gameObjectId;
            Normal = _normal;
        }
    }
}