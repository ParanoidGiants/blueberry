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
        public RaycastDirections RaycastDirections;

        public Vector3 GroundNormal;

        public List<Collider> currentContacts = new List<Collider>();

        private Camera cam;
        private new Rigidbody rigidbody;
        private Vector3 projectedUp;
        private Vector3 projectedForward;
        private Vector3 projectedRight;

        public bool IsGrounded { get; private set; }

        private void Start()
        {
            RaycastDirections = new RaycastDirections(transform);

            cam = FindObjectOfType<Camera>();
            rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            var playerPosition = transform.position;
            var cameraPosition = cam.transform.position;
            var collisionNormal = RaycastDirections.CurrentDown.Direction;
            var cameraRight = cam.transform.position + cam.transform.right;
            this.projectedRight = ChatGPT.IntersectingLine(playerPosition, cameraRight, cameraPosition, collisionNormal);
            var cameraUp = cam.transform.position + cam.transform.up;
            this.projectedUp = ChatGPT.IntersectingLine(playerPosition, cameraUp, cameraPosition, collisionNormal);

            //Debug.DrawRay(transform.position, cam.transform.up, Color.cyan);
            //Debug.DrawRay(transform.position, cam.transform.right, Color.red);
            //Debug.DrawRay(transform.position, projectedUp, Color.green);
            //Debug.DrawRay(transform.position, projectedRight, Color.magenta);
        }

        private void FixedUpdate()
        {
            RaycastDirections.Update();

            UpdateFall();
        }

        public void UpdateHeadMovement(Vector3 _playerInput)
        {
            if (!IsGrounded) return;

            var currentMoveDirection = projectedUp * _playerInput.y + projectedRight * _playerInput.x;
            if (currentMoveDirection.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(currentMoveDirection, RaycastDirections.CurrentUp.Direction);
            }
            rigidbody.MovePosition(transform.position + MoveSpeed * currentMoveDirection.normalized);
            RaycastDirections.UpdateMoveDirection(currentMoveDirection);
        }

        private void UpdateFall()
        {
            // TODO: Check FallSpeed again, when input vector can be projected on ground surface
            if (IsGrounded) return;

            RaycastDirections.Update();
            if (RaycastDirections.CurrentBack.IsDetecting)
            {
                rigidbody.MovePosition(transform.position + FallSpeed * RaycastDirections.CurrentBack.Direction);
            }
            else
            {
                rigidbody.MovePosition(transform.position + FallSpeed * RaycastDirections.CurrentDown.Direction);
            }

        }

        private void OnCollisionEnter(Collision collision)
        {
            bool isCollisionObjectClimbable = ((1 << collision.gameObject.layer) & WhatIsClimbable) != 0;
            if (!isCollisionObjectClimbable) return;

            currentContacts.Add(collision.collider);

            var collisionNormal = collision.contacts[0].normal;
            RaycastDirections.UpdateDownDirection(-collisionNormal);
            Debug.DrawRay(collision.contacts[0].point, collisionNormal, Color.magenta, 5f);

            transform.up = RaycastDirections.CurrentUp.Direction;
            rigidbody.velocity = Vector3.zero;
            IsGrounded = true;
        }

        private void OnCollisionExit(Collision collision)
        {
            bool isCollisionObjectClimbable = ((1 << collision.gameObject.layer) & WhatIsClimbable) != 0;
            if (!isCollisionObjectClimbable) return;

            currentContacts.Remove(collision.collider);
            IsGrounded = currentContacts.Count != 0;
        }
    }
}
