using System;
using UnityEngine;

namespace Creeper
{
    [Serializable]
    public class RaycastDirection
    {
        public Vector3 Direction;
        public bool IsGrounded;
        private Transform source;
        public Transform Other;

        private int ClimbablePhysicsLayer = LayerMask.GetMask("Climbable");

        public RaycastDirection(Transform _source, Vector3 _direction)
        {
            this.source = _source;
            Direction = _direction;
            IsGrounded = false;
        }

        internal void CheckForGround()
        {
            RaycastHit hit;
            Color color;
            if (Physics.Raycast(this.source.position, Direction, out hit, 1f, ClimbablePhysicsLayer))
            {
                color = Color.green;
                Other = hit.transform;
            }
            else
            {
                color = Color.red;
                Other = null;
            }
            // Debug.DrawRay(this.source.position, Direction, color);
        }
    }
}