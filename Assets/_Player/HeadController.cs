using System;
using UnityEngine;
using RootMath;

namespace Creeper
{
    #region RaycastLogic
    [Serializable]
    public class RaycastController
    {
        public bool IsGrounded { get { return ground.isDetecting; } }
        public bool IsSomethingBehind { get { return behind.isDetecting; } }
        public Vector3 UpDirection { get { return -ground.Direction; } }
        public Vector3 BehindDirection { get { return behind.Direction; } }

        public RaycastDirection ground;
        public RaycastDirection behind;

        public RaycastController(Transform source)
        {
            ground = new RaycastDirection(source, Vector3.down);
            behind = new RaycastDirection(source, Vector3.back);
            ground.DebugColor = Color.green;
            behind.DebugColor = Color.red;
        }

        public void UpdateBehind(Vector3 _behindDirection)
        {
            behind.Direction = _behindDirection;
        }

        public void UpdateDownDirection(Vector3 _downDirection)
        {
            ground.Direction = _downDirection;
        }

        public Vector3 GroundDirection { get { return ground.Direction; } }

        public void Update()
        {
            ground.Update();
            behind.Update();
        }
    }


    [Serializable]
    public class RaycastDirection
    {
        public Vector3 Direction;
        public bool isDetecting;
        public bool IsDetecting { get { return isDetecting; } }
        public Color DebugColor;

        private Transform source;
        private int whatIsClimbable;

        public RaycastDirection(Transform _source, Vector3 _direction)
        {
            this.source = _source;
            this.isDetecting = false;
            this.whatIsClimbable = HeadController.WHAT_IS_CLIMBABLE;
            Direction = _direction;
        }

        public void Update()
        {
            RaycastHit hit;
            this.isDetecting = Physics.Raycast(this.source.position, Direction, out hit, 100f, whatIsClimbable);
            if (this.isDetecting && Direction != hit.normal)
            {
                Direction = -hit.normal;
            }
        }

        public void DebugDraw()
        {
            Debug.DrawRay(this.source.position, Direction, DebugColor, 5f);
        }
    }
    #endregion RaycastLogic

    public class HeadController : MonoBehaviour
    {
        public static int WHAT_IS_CLIMBABLE;
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

        #region Movement
        [SerializeField] private float moveSpeed;
        private Vector3 inputDirection;
        private Axis projectedAxis;
        public Axis GetMovementAxese()
        {
            var camera = Camera.main.transform;
            var wallDirection = raycastController.GroundDirection;
            var axis = new Axis()
            {
                right = ChatGPT.IntersectingLine(transform.position, camera.right, camera.position, wallDirection),
                up = ChatGPT.IntersectingLine(transform.position, camera.up, camera.position, wallDirection)
            };
            return axis;
        }

        public void SetMovementDirection(Vector3 _inputDirection)
        {
            this.inputDirection = _inputDirection;
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
            this.rigidbody.MovePosition(transform.position + moveSpeed * moveDirection);
            if (this.inputDirection.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection, this.raycastController.UpDirection);
            }
        }
        #endregion Movement

        #region Falling
        [SerializeField] private RaycastController raycastController;
        [SerializeField] private bool isGrounded = false;
        [SerializeField] private int numberOfContacts = 0;
        private new Rigidbody rigidbody;

        private void UpdateFall()
        {
            if (isGrounded) return;
            if (this.raycastController.IsSomethingBehind)
            {
                this.rigidbody.MovePosition(transform.position + moveSpeed * this.raycastController.BehindDirection);
            }
            else
            {
                this.rigidbody.MovePosition(transform.position + moveSpeed * this.raycastController.GroundDirection);
            }
        }

        private void CheckCollisions(Collision _collision)
        {
            // Check if gameObject is climbable
            if (((1 << _collision.gameObject.layer) & WHAT_IS_CLIMBABLE) == 0) return;

            ContactPoint[] contacts = _collision.contacts;
            var contactCount = contacts.Length;
            if (contactCount == 0)
            {
                isGrounded = false;
                Debug.Log("SEARCH!");
            }
            else if (numberOfContacts != contactCount)
            {
                var contactPoint = contacts[contactCount - 1];
                isGrounded = true;
                Debug.Log("CONTACT!");
                var collisionNormal = contactPoint.normal;
                this.raycastController.UpdateDownDirection(-collisionNormal);
                Debug.DrawRay(contactPoint.point, collisionNormal, Color.magenta, 5f);
                transform.up = this.raycastController.UpDirection;
            }
            numberOfContacts = contactCount;
        }

        private void OnCollisionEnter(Collision _collision)
        {
            CheckCollisions(_collision);
        }

        private void OnCollisionStay(Collision _collision)
        {
            CheckCollisions(_collision);
        }

        private void OnCollisionExit(Collision _collision)
        {
            CheckCollisions(_collision);
        }
        #endregion Falling
    }
}
