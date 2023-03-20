using System;
using UnityEngine;

namespace Creeper
{
    [Serializable]
    public class RaycastController
    {
        public bool IsGrounded { get { return ground.isDetecting; } }
        public bool IsSomethingBehind { get { return behind.isDetecting; } }
        public Vector3 UpDirection { get { return -ground.Direction; } }
        public Vector3 BehindDirection { get { return behind.Direction; } }

        public RaycastDirection ground;
        public RaycastDirection behind;

        public RaycastController(Transform source)
        {
            ground = new RaycastDirection(source, Vector3.down);
            behind = new RaycastDirection(source, Vector3.back);
        }

        public void UpdateBehind(Vector3 _behindDirection)
        {
            behind.Direction = _behindDirection;
        }

        public void UpdateDownDirection(Vector3 _downDirection)
        {
            ground.Direction = _downDirection;
        }

        public Vector3 GroundDirection { get { return ground.Direction; } }

        public void Update()
        {
            ground.Update();
            behind.Update();
        }
    }


    [Serializable]
    public class RaycastDirection
    {
        public Vector3 Direction;
        public bool isDetecting;
        public bool IsDetecting { get { return isDetecting; } }

        private Transform source;
        private int whatIsClimbable;

        public RaycastDirection(Transform _source, Vector3 _direction)
        {
            this.source = _source;
            this.isDetecting = false;
            this.whatIsClimbable = HeadController.WHAT_IS_CLIMBABLE;
            Direction = _direction;
        }

        public void Update()
        {
            RaycastHit hit;
            Color color;
            this.isDetecting = Physics.Raycast(this.source.position, Direction, out hit, 1f, whatIsClimbable);
            if (this.isDetecting)
            {
                color = Color.green;
            }
            else
            {
                color = Color.red;
            }
            Debug.DrawRay(this.source.position, Direction, color);
        }
    }
}