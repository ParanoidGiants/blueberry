using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Creeper
{
    public class HeadController : MonoBehaviour
    {
        private const float MOVE_DIRECTION_THRESHOLD = -0.7f;

        public float MoveSpeed;
        public bool IsMoving;

        public float FallSpeed;
        public bool IsFalling;
        public float FallFor = 2f;
        public float FallTime = 0f;

        public LayerMask WhatIsClimbable;
        public RaycastDirection CurrentGround;
        public RaycastDirection CurrentForward;
        public RaycastDirection CurrentBack;
        private float currentBackReference = 0f;

        private new Rigidbody rigidbody;
        private Vector3 currentMoveDirection;
        private Vector3 cameraHandleForward;
        private Vector3 cameraHandleRight;

        private void Start()
        {
            CurrentGround = new RaycastDirection(Vector3.down);
            CurrentForward = new RaycastDirection(Vector3.forward);

            rigidbody = GetComponent<Rigidbody>();
            cameraHandleForward = Vector3.forward;
            cameraHandleRight = Vector3.right;
        }

        private void FixedUpdate()
        {
            CheckForGround();
            UpdateFall();
        }

        private void CheckForGround()
        {
            RaycastHit hit;
            Color color;
            if (Physics.Raycast(transform.position, CurrentGround.Direction, out hit, 1f, WhatIsClimbable))
            {
                color = Color.green;
                CurrentGround.Other = hit.transform;
            }
            else
            {
                color = Color.red;
                CurrentGround.Other = null;
            }
            Debug.DrawRay(transform.position, CurrentGround.Direction, color);
        }

        private void UpdateFall()
        {
            bool wasFalling = IsFalling;
            IsFalling = CurrentGround.Other == null;
            if (!wasFalling && IsFalling)
            {
                UpdateBackDirection();
            }
            else if (wasFalling && !IsFalling)
            {
                FreezeRigidbody();
            }
            var velocity = FallSpeed * CurrentGround.Direction;
            if (IsFalling)
            {
                // Search for ground
                RaycastHit hit;
                if (Physics.Raycast(transform.position, CurrentBack.Direction, out hit, 1f, WhatIsClimbable))
                {
                    CurrentBack.Other = hit.transform;
                    CurrentGround = CurrentBack;
                    RotatePlayerAndCamera(true);
                    FallTime = 0f;
                }
                else if (Physics.Raycast(transform.position, -currentMoveDirection, out hit, 10f, WhatIsClimbable))
                {
                    velocity -= 4f * FallSpeed * currentMoveDirection;
                }
            }

            rigidbody.AddForce(velocity, ForceMode.Force);
            rigidbody.velocity = Vector3.ClampMagnitude(rigidbody.velocity, 2f);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (IsFalling) return;

            Vector3 collisionNormal = collision.GetContact(0).normal;
            float dotProduct = Vector3.Dot(collisionNormal, currentMoveDirection.normalized);

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
                CurrentGround = new RaycastDirection(-collisionNormal);
                CurrentForward = new RaycastDirection(transform.up);
                RotatePlayerAndCamera(false);
            }
        }

        public Vector3 GetWorldDirection(Vector2 _inputDirection)
        {
            var directionWorldSpace = cameraHandleRight * _inputDirection.x + cameraHandleForward * _inputDirection.y;
            return directionWorldSpace;
        }

        public void UpdateHead(Vector3 _directionWorldSpace)
        {
            if (IsFalling) return;
            currentMoveDirection = _directionWorldSpace;
            rigidbody.MovePosition(transform.position + MoveSpeed * currentMoveDirection);
        }

        private void FreezeRigidbody()
        {
            rigidbody.velocity = Vector3.zero;
        }

        private void RotatePlayerAndCamera(bool isFalling)
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
                if (isFalling)
                {
                    _cameraForward = -_cameraForward;
                }
                _cameraRight = -_cameraRight;
            }
            else if (_cameraUp.y == 1f)
            {
                if (!isFalling)
                {
                    _cameraForward = -_cameraForward;
                }
                _cameraRight = -_cameraRight;
            }
            _cameraForward.y = Mathf.Abs(_cameraForward.y);
            targetRotation = Quaternion.LookRotation(_cameraForward, _cameraUp);
            FindObjectOfType<FollowTarget>().InitRotate(targetRotation, FreezeRigidbody);
            this.cameraHandleForward = targetRotation * Vector3.forward;
            this.cameraHandleRight = targetRotation * Vector3.right;
        }

        private void UpdateBackDirection()
        {
            currentBackReference = 0f;
            var fallingDirection = CurrentGround.Direction;
            Vector3 backDirection = Vector3.zero;
            if (fallingDirection.x != 0f)
            {
                var y = Mathf.Abs(rigidbody.velocity.y);
                var z = Mathf.Abs(rigidbody.velocity.z);
                if (y > z && y > currentBackReference)
                {
                    currentBackReference = rigidbody.velocity.y;
                    backDirection = -Mathf.Sign(rigidbody.velocity.y) * Vector3.up;
                }
                else if (z > y && z > currentBackReference)
                {
                    currentBackReference = rigidbody.velocity.z;
                    backDirection = -Mathf.Sign(rigidbody.velocity.z) * Vector3.forward;
                }
            }
            else if (fallingDirection.y != 0f)
            {
                var x = Mathf.Abs(rigidbody.velocity.x);
                var z = Mathf.Abs(rigidbody.velocity.z);
                if (x > z && x > currentBackReference)
                {
                    currentBackReference = rigidbody.velocity.x;
                    backDirection = -Mathf.Sign(rigidbody.velocity.x) * Vector3.right;
                }
                else if (z > x && z > currentBackReference)
                {
                    currentBackReference = rigidbody.velocity.z;
                    backDirection = -Mathf.Sign(rigidbody.velocity.z) * Vector3.forward;
                }
            }
            else if (fallingDirection.z != 0f)
            {
                var y = Mathf.Abs(rigidbody.velocity.y);
                var x = Mathf.Abs(rigidbody.velocity.x);
                if (y > x && y > currentBackReference)
                {
                    currentBackReference = rigidbody.velocity.y;
                    backDirection = -Mathf.Sign(rigidbody.velocity.y) * Vector3.up;
                }
                else if (x > y && x > currentBackReference)
                {
                    currentBackReference = rigidbody.velocity.x;
                    backDirection = -Mathf.Sign(rigidbody.velocity.x) * Vector3.right;
                }
            }
            if (backDirection == Vector3.zero) return;

            CurrentBack = new RaycastDirection(backDirection);
            CurrentForward = CurrentGround;
        }
    }
}
