using System;
using System.Linq;
using System.Collections.Generic;
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
    public class HeadController : MonoBehaviour
    {
        public float MoveSpeed;
        public LayerMask WhatIsClimbable;
        public bool IsMoving;
        public bool IsFalling;

        public RaycastDirection CurrentGround;
        public List<RaycastDirection> raycastDirections;
        private Rigidbody rigidbody;

        private void Start()
        {
            CurrentGround = new RaycastDirection(Vector3.down);
            raycastDirections = new List<RaycastDirection>() {
                new RaycastDirection(Vector3.forward),
                new RaycastDirection(Vector3.back),
                new RaycastDirection(Vector3.up),
                CurrentGround,
                new RaycastDirection(Vector3.left),
                new RaycastDirection(Vector3.right)
            };
            rigidbody = GetComponent<Rigidbody>();
            UpdateRaycasts();
        }

        public void UpdateHead(Vector3 directionLocalSpace)
        {
            if (IsFalling) return;

            var directionWorldSpace = transform.right * directionLocalSpace.x + transform.forward * directionLocalSpace.z;
            moveDirection = directionWorldSpace;
            transform.position += MoveSpeed * directionWorldSpace;
        }

        private void Update()
        {
            IsFalling = CurrentGround == null || CurrentGround.Other == null;


            if (IsFalling)
            {
                rigidbody.MovePosition(transform.position + 10f * MoveSpeed * CurrentGround.Direction);
                FindNewGround();
            }

            UpdateRaycasts();
        }

        private void FindNewGround()
        {
            Debug.Log("Seraching new ground!");
            foreach (var raycastDirection in raycastDirections)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, raycastDirection.Direction, out hit, 1f, WhatIsClimbable))
                {
                    raycastDirection.Other = hit.transform;

                    CurrentGround = raycastDirection;
                    transform.up = -CurrentGround.Direction;
                    IsFalling = false;
                    return;
                }
            }
        }

        private void UpdateRaycasts()
        {
            foreach (var raycastDirection in raycastDirections)
            {
                RaycastHit hit;
                Color color;
                if (Physics.Raycast(transform.position, raycastDirection.Direction, out hit, 1f, WhatIsClimbable))
                {
                    color = Color.green;
                    raycastDirection.Other = hit.transform;
                }
                else
                {
                    color = Color.red;
                    raycastDirection.Other = null;
                }
                Debug.DrawRay(transform.position, raycastDirection.Direction, color);
            }
        }

        private const float moveDirectionThreshold = 0.7f;
        private Vector3 moveDirection;

        private void OnCollisionEnter(Collision collision)
        {
            if (IsFalling) return;

            Vector3 objectDirection = (collision.GetContact(0).point - transform.position).normalized;
            float dotProduct = Vector3.Dot(objectDirection, moveDirection.normalized);
            Debug.Log(dotProduct);
            Debug.Log(objectDirection + " " + moveDirection);

            bool canClimb = 
                // Is Object climbable
                ((1 << collision.gameObject.layer) & WhatIsClimbable) != 0
                // Is Object different from current ground
                && collision.transform != CurrentGround.Other
                // Am I moving
                && IsMoving
                // Am I moving into its direction
                && dotProduct >= moveDirectionThreshold;

            if (canClimb)
            {
                transform.up = -objectDirection;
                CurrentGround = raycastDirections.FirstOrDefault(x => Vector3.Dot(x.Direction, -transform.up) > 0.1f);
            }
        }
    }
}
