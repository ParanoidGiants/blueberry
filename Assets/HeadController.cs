using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Creeper
{
    public class HeadController : MonoBehaviour
    {
        public float MoveSpeed;
        public float FallSpeed;
        public LayerMask WhatIsClimbable;
        public bool IsMoving;
        public bool IsFalling;

        public RaycastDirection CurrentGround;
        public RaycastDirection CurrentForward;
        public Vector3 lastPosition;
        public bool IsRotating { get { return Cam.IsRotating; } }
        public Plane GetXZPlane()
        {
            var a = transform.position;
            var b = transform.position + cameraForward;
            var c = transform.position - cameraRight;
            return new Plane(a, b, c);
        }

        public List<RaycastDirection> raycastDirections;
        private new Rigidbody rigidbody;

        public FollowTarget Cam;
        public Vector3 cameraForward;
        public Vector3 cameraRight;

        private const float moveDirectionThreshold = -0.7f;
        private Vector3 moveDirection;

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
            cameraForward = Vector3.forward;
            cameraRight = Vector3.right;
        }

        private void Update()
        {
            UpdateRaycasts();
            UpdateFall();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (IsFalling) return;

            Vector3 collisionNormal = collision.GetContact(0).normal;
            float dotProduct = Vector3.Dot(collisionNormal, moveDirection.normalized);
            //Debug.Log(dotProduct);
            //Debug.Log(collisionNormal + " " + moveDirection);

            bool canClimb =
                // Is Object climbable
                ((1 << collision.gameObject.layer) & WhatIsClimbable) != 0
                // Is Object different from current ground
                && collision.transform != CurrentGround.Other
                // Am I moving
                && IsMoving
                // Am I moving into its direction
                && dotProduct <= moveDirectionThreshold;

            if (canClimb)
            {
                transform.up = -collisionNormal;
                CurrentGround = raycastDirections.FirstOrDefault(x => Vector3.Dot(x.Direction, -transform.up) > 0.1f);
            }
        }

        public void UpdateHead(Vector2 inputDirection)
        {
            if (IsFalling || Cam.IsRotating) return;

            var directionWorldSpace = cameraRight * inputDirection.x + cameraForward * inputDirection.y;
            moveDirection = directionWorldSpace;
            transform.position += MoveSpeed * directionWorldSpace;
        }


        private void UpdateFall()
        {
            rigidbody.AddForce(FallSpeed * CurrentGround.Direction, ForceMode.Force);
            rigidbody.velocity = Vector3.ClampMagnitude(rigidbody.velocity, 2f);

            bool wasFalling = IsFalling;
            IsFalling = CurrentGround == null || CurrentGround.Other == null;
            if (!wasFalling && IsFalling)
            {
                CurrentForward = CurrentGround;
            }
            else if (wasFalling && !IsFalling)
            {
                //Debug.Log("FREEZE!");
                rigidbody.velocity = Vector3.zero;
            }

            if (!IsFalling) return;
            

            // Search for ground
            foreach (var raycastDirection in raycastDirections)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, raycastDirection.Direction, out hit, 1f, WhatIsClimbable))
                {
                    raycastDirection.Other = hit.transform;
                    CurrentGround = raycastDirection;
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

        private void Rotate()
        {
            transform.up = -CurrentGround.Direction;

            Quaternion targetRotation;
            var _cameraForward = CurrentForward.Direction;
            _cameraForward.y = Mathf.Abs(_cameraForward.y);



            var _cameraUp = transform.up;
            targetRotation = Quaternion.LookRotation(_cameraForward, _cameraUp);
            FindObjectOfType<FollowTarget>().InitRotate(targetRotation);

            var _cameraRight = Vector3.Cross(_cameraUp, _cameraForward);
            Debug.Log("---------------------------");
            Debug.Log("CurrRight: " + _cameraRight);
            Debug.Log("CurrUp: " + _cameraUp);
            Debug.Log("CurrForward: " + _cameraForward);

            this.cameraForward = targetRotation * Vector3.forward;
            this.cameraRight = targetRotation * Vector3.right;
        }
    }
}
