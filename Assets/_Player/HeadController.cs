using System.Collections.Generic;
using UnityEngine;

namespace Creeper
{
    public class HeadController : MonoBehaviour
    {
        public float MoveSpeed;
        public static int WHAT_IS_CLIMBABLE;

        private Vector3 inputDirection;
        private Vector3 projectedRight;
        private Vector3 projectedUp;
        private RaycastController raycastController;
        private List<Collider> currentContacts = new List<Collider>();
        private new Rigidbody rigidbody;

        public bool IsGrounded { get; private set; }
        public Vector3 GroundDirection { get { return raycastController.GroundDirection; } }

        private void Start()
        {
            WHAT_IS_CLIMBABLE = LayerMask.GetMask("Climbable");

            this.raycastController = new RaycastController(transform);
            this.rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            UpdateFall();
            UpdateMovement();
        }
        private void UpdateMovement()
        {
            if (!IsGrounded) return;

            (this.projectedRight, this.projectedUp) = GetMovementAxese();
            var moveDirection = this.projectedRight * this.inputDirection.x + this.projectedUp * this.inputDirection.y;
            moveDirection = moveDirection.normalized;
            this.rigidbody.MovePosition(transform.position + MoveSpeed * moveDirection);
            this.raycastController.UpdateBehind(-moveDirection);

            transform.rotation = Quaternion.LookRotation(moveDirection, this.raycastController.UpDirection);
        }

        public void SetMovementDirection(Vector3 _inputDirection)
        {
            this.inputDirection = _inputDirection;
        }

        private void UpdateFall()
        {
            if (IsGrounded) return;

            this.raycastController.Update();
            if (this.raycastController.IsSomethingBehind)
            {
                this.rigidbody.MovePosition(transform.position + MoveSpeed * this.raycastController.BehindDirection);
            }
            else
            {
                this.rigidbody.MovePosition(transform.position + MoveSpeed * this.raycastController.GroundDirection);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!IsLayerClimbable(collision.gameObject.layer)) return;

            this.currentContacts.Add(collision.collider);

            var collisionNormal = collision.contacts[0].normal;
            this.raycastController.UpdateDownDirection(-collisionNormal);
            Debug.DrawRay(collision.contacts[0].point, collisionNormal, Color.magenta, 5f);

            transform.up = this.raycastController.UpDirection;
            this.rigidbody.velocity = Vector3.zero;
            IsGrounded = true;
        }

        private void OnCollisionExit(Collision collision)
        {
            if (!IsLayerClimbable(collision.gameObject.layer)) return;

            this.currentContacts.Remove(collision.collider);
            IsGrounded = this.currentContacts.Count != 0;
        }

        private bool IsLayerClimbable(int _layer)
        {
            return ((1 << _layer) & WHAT_IS_CLIMBABLE) != 0;
        }

        public (Vector3, Vector3) GetMovementAxese()
        {
            var camera = Camera.main.transform;
            var wallDirection = GroundDirection;
            return (
                ChatGPT.IntersectingLine(transform.position, camera.right, camera.position, wallDirection),
                ChatGPT.IntersectingLine(transform.position, camera.up, camera.position, wallDirection)
            );
        }
    }
}
