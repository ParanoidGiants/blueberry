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
        public RaycastDirection CurrentForward;
        public Vector3 lastPosition;
        public List<RaycastDirection> raycastDirections;
        private Rigidbody rigidbody;
        public FollowTarget Cam;

        public Vector3 v_currentForward;
        public Vector3 v_currentRight;

        private void Start()
        {
            CurrentGround = new RaycastDirection(Vector3.down);
            CurrentForward = new RaycastDirection(Vector3.forward);
            raycastDirections = new List<RaycastDirection>() {
                CurrentForward,
                new RaycastDirection(Vector3.back),
                new RaycastDirection(Vector3.up),
                CurrentGround,
                new RaycastDirection(Vector3.left),
                new RaycastDirection(Vector3.right)
            };
            rigidbody = GetComponent<Rigidbody>();
            UpdateRaycasts();
            v_currentForward = Vector3.forward;
            v_currentRight = Vector3.right;
        }

        public void UpdateHead(Vector3 directionLocalSpace)
        {
            if (IsFalling || Cam.IsRotating) return;

            var directionWorldSpace = v_currentRight * directionLocalSpace.x + v_currentForward * directionLocalSpace.z;
            moveDirection = directionWorldSpace;
            transform.position += MoveSpeed * directionWorldSpace;
        }

        private void Update()
        {

            IsFalling = CurrentGround == null || CurrentGround.Other == null;


            if (IsFalling)
            {
                rigidbody.MovePosition(transform.position + 10f * MoveSpeed * CurrentGround.Direction);
                CurrentForward = CurrentGround;
                FindNewGround();
            }

            UpdateRaycasts();
        }

        private void FindNewGround()
        {
            foreach (var raycastDirection in raycastDirections)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, raycastDirection.Direction, out hit, 1f, WhatIsClimbable))
                {
                    raycastDirection.Other = hit.transform;
                    CurrentGround = raycastDirection;
                    transform.up = -CurrentGround.Direction;
                    IsFalling = false;
                    Rotate();
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
            //Debug.Log(dotProduct);
            //Debug.Log(objectDirection + " " + moveDirection);

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

        private void Rotate()
        {
            Quaternion targetRotation;
            var currentForward = CurrentForward.Direction;
            var currentUp = transform.up;
            Debug.Log("CurrForward: " + currentForward);
            Debug.Log("CurrUp: " + currentUp);
            if (currentForward.y < 0f && currentUp.z > 0f)
            {
                targetRotation = Quaternion.LookRotation(-transform.up, Vector3.down);;
            }
            else if (currentForward.x > 0f && currentUp.z > 0f)
            {
                targetRotation = Quaternion.LookRotation(-transform.up, Vector3.right);
            }
            else if (currentForward.x < 0f && currentUp.z > 0f)
            {
                targetRotation = Quaternion.LookRotation(-transform.up, Vector3.left);
            }
            else if (currentForward.y > 0f && currentUp.z > 0f)
            {
                targetRotation = Quaternion.LookRotation(-transform.up, Vector3.up);
            }
            else if (currentForward.y < 0f && currentUp.z < 0f)
            {
                targetRotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            }
            //else if (currentForward.x > 0f && currentUp.z > 0f)
            //{
            //    targetRotation = Quaternion.LookRotation(-transform.up, Vector3.right);
            //    FindObjectOfType<FollowTarget>().InitRotate(targetRotation);
            //}
            //else if (currentForward.x < 0f && currentUp.z > 0f)
            //{
            //    targetRotation = Quaternion.LookRotation(-transform.up, Vector3.left);
            //    FindObjectOfType<FollowTarget>().InitRotate(targetRotation);
            //}
            //else if (currentForward.y > 0f && currentUp.z > 0f)
            //{
            //    targetRotation = Quaternion.LookRotation(-transform.up, Vector3.up);
            //    FindObjectOfType<FollowTarget>().InitRotate(targetRotation);
            //}
            /// 
            else
            {
                targetRotation = Quaternion.LookRotation(-transform.up, Vector3.forward);
                FindObjectOfType<FollowTarget>().InitRotate(targetRotation);
            }
            v_currentForward = targetRotation * Vector3.up;
            v_currentRight = targetRotation * Vector3.right;
            FindObjectOfType<FollowTarget>().InitRotate(targetRotation);
        }
    }
}
