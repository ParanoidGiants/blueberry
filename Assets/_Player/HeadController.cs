using System;
using System.Collections.Generic;
using UnityEngine;

namespace Creeper
{
    [Serializable]
    public class Axis
    {
        public Vector3 up;
        public Vector3 right;
    }
    public class HeadController : MonoBehaviour
    {
        public float MoveSpeed;
        public static int WHAT_IS_CLIMBABLE;

        private Vector3 inputDirection;

        [SerializeField]
        private Axis projectedAxis;
        private RaycastController raycastController;
        private new Rigidbody rigidbody;

        [SerializeField]
        private bool isGrounded = false;
        public Vector3 GroundDirection { get { return raycastController.GroundDirection; } }

        private void Start()
        {
            WHAT_IS_CLIMBABLE = LayerMask.GetMask("Climbable");
            this.raycastController = new RaycastController(transform);
            this.rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            this.rigidbody.velocity = Vector3.zero;
            this.raycastController.Update();
            UpdateFall();
            UpdateMovement();
        }

        private void UpdateMovement()
        {
            if (!isGrounded) return;

            this.projectedAxis = GetMovementAxese();
            var moveDirection = Vector3.Normalize(
                this.projectedAxis.right * this.inputDirection.x
                + this.projectedAxis.up * this.inputDirection.y
            );
            this.raycastController.UpdateBehind(-moveDirection);
            this.rigidbody.MovePosition(transform.position + MoveSpeed * moveDirection);
            if (this.inputDirection.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection, this.raycastController.UpDirection);
            }
        }

        private void UpdateFall()
        {
            if (isGrounded) return;
            if (this.raycastController.IsSomethingBehind)
            {
                this.rigidbody.MovePosition(transform.position + MoveSpeed * this.raycastController.BehindDirection);
            }
            else
            {
                this.rigidbody.MovePosition(transform.position + MoveSpeed * this.raycastController.GroundDirection);
            }
        }

        public void SetMovementDirection(Vector3 _inputDirection) { this.inputDirection = _inputDirection; }

        int numberOfContacts = 0;
        private void CheckCollisions(Collision _collision)
        {
            // Check if gameObject is climbable
            if (((1 << _collision.gameObject.layer) & WHAT_IS_CLIMBABLE) != 0) return;

            ContactPoint[] _contacts = _collision.contacts;
            var contactCount = _contacts.Length;
            if (contactCount == 0)
            {
                isGrounded = false;
                Debug.Log("SEARCH!");
            }
            else if (numberOfContacts != contactCount)
            {
                var contactPoint = _contacts[contactCount - 1];
                isGrounded = true;
                Debug.Log("CONTACT!");
                var collisionNormal = contactPoint.normal;
                this.raycastController.UpdateDownDirection(-collisionNormal);
                Debug.DrawRay(contactPoint.point, collisionNormal, Color.magenta, 5f);
                transform.up = this.raycastController.UpDirection;
            }

            numberOfContacts = contactCount;
        }

        private void OnCollisionEnter(Collision _collision) { CheckCollisions(_collision); }

        private void OnCollisionStay(Collision _collision) { CheckCollisions(_collision); }

        private void OnCollisionExit(Collision _collision) { CheckCollisions(_collision); }

        public Axis GetMovementAxese()
        {
            var camera = Camera.main.transform;
            var wallDirection = GroundDirection;
            var axis = new Axis()
            {
                right = ChatGPT.IntersectingLine(transform.position, camera.right, camera.position, wallDirection),
                up = ChatGPT.IntersectingLine(transform.position, camera.up, camera.position, wallDirection)
            };
            return axis;
        }

    }
}
