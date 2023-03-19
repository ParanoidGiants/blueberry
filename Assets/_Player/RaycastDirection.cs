using System;
using UnityEngine;

namespace Creeper
{
    [Serializable]
    public class RaycastDirections
    {
        public RaycastDirection CurrentUp;
        public RaycastDirection CurrentDown;
        public RaycastDirection CurrentLeft;
        public RaycastDirection CurrentRight;
        public RaycastDirection CurrentForward;
        public RaycastDirection CurrentBack;

        public RaycastDirections(Transform source)
        {
            CurrentUp = new RaycastDirection(source, Vector3.up);
            CurrentDown = new RaycastDirection(source, Vector3.down);
            CurrentLeft = new RaycastDirection(source, Vector3.left);
            CurrentRight = new RaycastDirection(source, Vector3.right);
            CurrentForward = new RaycastDirection(source, Vector3.forward);
            CurrentBack = new RaycastDirection(source, Vector3.back);
        }

        public void UpdateMoveDirection(Vector3 moveDirection)
        {
            CurrentForward.Direction = moveDirection;
            CurrentBack.Direction = -moveDirection;
            CurrentLeft.Direction = Vector3.Cross(CurrentForward.Direction, CurrentDown.Direction);
        }

        public void UpdateDownDirection(Vector3 downDirection)
        {
            CurrentDown.Direction = downDirection;
            CurrentUp.Direction = -downDirection;
        }

        public void Update()
        {
            CurrentDown.Update();
            CurrentLeft.Update();
            CurrentRight.Update();
            CurrentForward.Update();
            CurrentBack.Update();
        }

        public bool IsGrounded()
        {
            return CurrentDown.IsDetecting;
        }
    }


    [Serializable]
    public class RaycastDirection
    {
        public Vector3 Direction;
        public bool IsDetecting;
        public float Distance;
        public Transform Other;

        private Transform source;
        private int ClimbablePhysicsLayer = LayerMask.GetMask("Climbable");

        public RaycastDirection(Transform _source, Vector3 _direction)
        {
            this.source = _source;
            Direction = _direction;
            IsDetecting = false;
        }

        internal void Update()
        {
            RaycastHit hit;
            Color color;
            IsDetecting = Physics.Raycast(this.source.position, Direction, out hit, 1f, ClimbablePhysicsLayer);
            if (IsDetecting)
            {
                color = Color.green;
                Other = hit.transform;
                Distance = hit.distance;
            }
            else
            {
                color = Color.red;
                Other = null;
                Distance = 0f;
            }
            Debug.DrawRay(this.source.position, Direction, color);
        }
    }
}