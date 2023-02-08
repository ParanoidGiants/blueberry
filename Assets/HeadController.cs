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
        public List<RaycastDirection> raycastDirections;


        private const float MOVE_DIRECTION_THRESHOLD = -0.7f;
        private new Rigidbody rigidbody;
        private Vector3 moveDirection;
        private Vector3 cameraHandleForward;
        private Vector3 cameraHandleRight;

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
            cameraHandleForward = Vector3.forward;
            cameraHandleRight = Vector3.right;
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
                && dotProduct <= MOVE_DIRECTION_THRESHOLD;

            if (canClimb)
            {
                Debug.Log(-collisionNormal);
                CurrentGround = raycastDirections.FirstOrDefault(x => x.Direction == -collisionNormal);
                CurrentForward = raycastDirections.FirstOrDefault(x => x.Direction == transform.up);
                RotatePlayerAndCamera();
            }
        }

        public void UpdateHead(Vector2 inputDirection)
        {
            if (IsFalling) return;

            var directionWorldSpace = cameraHandleRight * inputDirection.x + cameraHandleForward * inputDirection.y;
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
                FreezeRigidbody();
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
                    RotatePlayerAndCamera();
                    return;
                }
            }
        }

        private void FreezeRigidbody()
        {
            //Debug.Log("FREEZE!");
            rigidbody.velocity = Vector3.zero;
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

        private void RotatePlayerAndCamera()
        {
            transform.up = -CurrentGround.Direction;
            Quaternion targetRotation;
            var _cameraForward = CurrentForward.Direction;
            var _cameraUp = transform.up;
            var _cameraRight = Vector3.Cross(_cameraUp, _cameraForward);
            
            if (_cameraRight.y != 0f)
            {
                var temp = _cameraForward;
                _cameraForward = _cameraRight;
                _cameraRight = _cameraForward;
            }
            else if (_cameraUp.y == -1f)
            {
                _cameraForward = -_cameraForward;
                _cameraRight = -_cameraRight;
            }
            _cameraForward.y = Mathf.Abs(_cameraForward.y);
            targetRotation = Quaternion.LookRotation(_cameraForward, _cameraUp);
            FindObjectOfType<FollowTarget>().InitRotate(targetRotation, FreezeRigidbody);

            //Debug.Log("---------------------------");
            //Debug.Log("CurrRight: " + _cameraRight);
            //Debug.Log("CurrUp: " + _cameraUp);
            //Debug.Log("CurrForward: " + _cameraForward);

            this.cameraHandleForward = targetRotation * Vector3.forward;
            this.cameraHandleRight = targetRotation * Vector3.right;
        }
    }
}
