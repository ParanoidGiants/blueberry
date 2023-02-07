using System;
using UnityEngine;

namespace Creeper
{
    [Serializable]
    public class RaycastDirection
    {
        public Vector3 Direction;
        public bool IsGrounded;
        public Transform Other;

        public RaycastDirection(Vector3 _dircetion)
        {
            Direction = _dircetion;
            IsGrounded = false;
        }
    }
}