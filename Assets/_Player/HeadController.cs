using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Creeper
{
    public class HeadController : MonoBehaviour
    {
        public static int WHAT_IS_CLIMBABLE = LayerMask.GetMask("Climbable");

        public float MoveSpeed;
        public float FallSpeed;

        private List<Collider> currentContacts = new List<Collider>();

        private RaycastController raycastController;
        private Camera cam;
        private new Rigidbody rigidbody;
        private Vector3 projectedUp;
        private Vector3 projectedRight;

        public bool IsGrounded { get; private set; }

        private void Start()
        {
            this.raycastController = new RaycastController(transform);

            this.cam = FindObjectOfType<Camera>();
            this.rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            var playerPosition = transform.position;
            var cameraPosition = this.cam.transform.position;
            var currentGroundDirection = this.raycastController.GroundDirection;

            var cameraRight = this.cam.transform.position + this.cam.transform.right;
            this.projectedRight = ChatGPT.IntersectingLine(playerPosition, cameraRight, cameraPosition, currentGroundDirection);
            var cameraUp = this.cam.transform.position + this.cam.transform.up;
            this.projectedUp = ChatGPT.IntersectingLine(playerPosition, cameraUp, cameraPosition, currentGroundDirection);

            //Debug.DrawRay(transform.position, this.cam.transform.up, Color.cyan);
            //Debug.DrawRay(transform.position, this.cam.transform.right, Color.red);
            //Debug.DrawRay(transform.position, this.projectedUp, Color.green);
            //Debug.DrawRay(transform.position, this.projectedRight, Color.magenta);
        }

        private void FixedUpdate()
        {
            this.raycastController.Update();

            UpdateFall();
        }

        public void UpdateHeadMovement(Vector3 _playerInput)
        {
            if (!IsGrounded) return;

            var currentMoveDirection = this.projectedUp * _playerInput.y + this.projectedRight * _playerInput.x;
            if (currentMoveDirection.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(currentMoveDirection, this.raycastController.UpDirection);
            }
            this.rigidbody.MovePosition(transform.position + MoveSpeed * currentMoveDirection.normalized);
            this.raycastController.UpdateBehind(currentMoveDirection);
        }

        private void UpdateFall()
        {
            if (IsGrounded) return;

            this.raycastController.Update();
            if (this.raycastController.IsSomethingBehind)
            {
                this.rigidbody.MovePosition(transform.position + FallSpeed * this.raycastController.BehindDirection);
            }
            else
            {
                this.rigidbody.MovePosition(transform.position + FallSpeed * this.raycastController.GroundDirection);
            }

        }

        private void OnCollisionEnter(Collision collision)
        {
            bool isCollisionObjectClimbable = ((1 << collision.gameObject.layer) & WHAT_IS_CLIMBABLE) != 0;
            if (!isCollisionObjectClimbable) return;

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
            bool isCollisionObjectClimbable = ((1 << collision.gameObject.layer) & WHAT_IS_CLIMBABLE) != 0;
            if (!isCollisionObjectClimbable) return;

            this.currentContacts.Remove(collision.collider);
            IsGrounded = this.currentContacts.Count != 0;
        }
    }
}
