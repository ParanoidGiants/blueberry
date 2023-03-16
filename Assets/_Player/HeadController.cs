using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Creeper
{
    public class HeadController : MonoBehaviour
    {
        private const float MOVE_DIRECTION_THRESHOLD = -0.7f;

        public float MoveSpeed;

        public float FallSpeed;
        public float HookSpeed;
        public float GroundCheckLength;

        public LayerMask WhatIsClimbable;
        public RaycastDirection CurrentGround;
        public RaycastDirection CurrentBack;
        public RaycastDirection CurrentRight;
        public RaycastDirection CurrentLeft;

        public Vector3 GroundNormal;
        public RaycastDirection CurrentForward;

        public List<Collider> currentContacts = new List<Collider>();

        private Camera cam;
        private new Rigidbody rigidbody;
        private Vector3 projectedUp;
        private Vector3 projectedForward;
        private Vector3 projectedRight;

        public bool IsGrounded { get; private set; }

        private void Start()
        {
            CurrentGround = new RaycastDirection(transform, Vector3.down);
            CurrentForward = new RaycastDirection(transform, Vector3.forward);
            CurrentRight = new RaycastDirection(transform, Vector3.right);
            CurrentLeft = new RaycastDirection(transform, Vector3.left);
            CurrentBack = new RaycastDirection(transform, Vector3.back);

            cam = FindObjectOfType<Camera>();
            rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            var playerPosition = transform.position;
            var cameraPosition = cam.transform.position;
            var collisionNormal = CurrentGround.Direction;
            var cameraRight = cam.transform.position + cam.transform.right;
            this.projectedRight = ChatGPT.IntersectingLine(playerPosition, cameraRight, cameraPosition, collisionNormal);
            var cameraUp = cam.transform.position + cam.transform.up;
            this.projectedUp = ChatGPT.IntersectingLine(playerPosition, cameraUp, cameraPosition, collisionNormal);

            Debug.DrawRay(transform.position, cam.transform.up, Color.cyan);
            Debug.DrawRay(transform.position, cam.transform.right, Color.red);
            Debug.DrawRay(transform.position, projectedUp, Color.green);
            Debug.DrawRay(transform.position, projectedRight, Color.magenta);
        }

        private void FixedUpdate()
        {
            CurrentGround.CheckForGround();

            UpdateFall();
        }

        public void UpdateHeadMovement(Vector3 _playerInput)
        {
            if (!IsGrounded) return;

            var currentMoveDirection = projectedUp * _playerInput.y + projectedRight * _playerInput.x;
            if (currentMoveDirection.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(currentMoveDirection, -CurrentGround.Direction);
            }
            rigidbody.MovePosition(transform.position + MoveSpeed * currentMoveDirection.normalized);
            CurrentBack.Direction = -currentMoveDirection;
        }

        private void UpdateFall()
        {
            if (!IsGrounded)
            {
                CurrentBack.CheckForGround();

                if (CurrentBack.IsGrounded)
                {
                    rigidbody.AddForce(HookSpeed * CurrentBack.Direction * Time.deltaTime, ForceMode.Acceleration);
                }
                //CurrentLeft.CheckForGround();
                //CurrentRight.CheckForGround();
                //if (CurrentLeft.IsGrounded)
                //{
                //    rigidbody.AddForce(HookSpeed * CurrentLeft.Direction * Time.deltaTime, ForceMode.Acceleration);
                //}
                //if (CurrentRight.IsGrounded)
                //{
                //    rigidbody.AddForce(HookSpeed * CurrentRight.Direction * Time.deltaTime, ForceMode.Acceleration);
                //}
            }

            // TODO: Check FallSpeed again, when input vector can be projected on ground surface
            rigidbody.AddForce(FallSpeed * CurrentGround.Direction * Time.deltaTime, ForceMode.Acceleration);

        }

        private void OnCollisionEnter(Collision collision)
        {
            if (((1 << collision.gameObject.layer) & WhatIsClimbable) != 0)
            {
                var collisionNormal = collision.contacts[0].normal;
                Debug.DrawRay(collision.contacts[0].point, collisionNormal, Color.magenta, 5f);
                CurrentGround.Direction = -collisionNormal;
                CurrentGround.Other = collision.transform;
                currentContacts.Add(collision.collider);
                transform.up = -CurrentGround.Direction;

                if (!IsGrounded)
                {
                    rigidbody.velocity = Vector3.zero;
                }

                IsGrounded = true;
            }
        }
        private void OnCollisionExit(Collision collision)
        {
            if (((1 << collision.gameObject.layer) & WhatIsClimbable) != 0)
            {
                currentContacts.Remove(collision.collider);
                IsGrounded = currentContacts.Count != 0;
            }
        }
    }
}
