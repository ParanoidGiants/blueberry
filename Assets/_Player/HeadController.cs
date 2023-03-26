using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RootMath;

namespace Creeper
{
    public class HeadController : MonoBehaviour
    {
        public static int WHAT_IS_CLIMBABLE;
        private void Start()
        {
            WHAT_IS_CLIMBABLE = LayerMask.GetMask("Climbable");
            this.rigidbody = GetComponent<Rigidbody>();
        }
        private void Update()
        {
            // Debug.DrawRay(transform.position, -this.groundDirection, Color.magenta, 5f);
        }
        private void FixedUpdate()
        {
            this.rigidbody.velocity = Vector3.zero;
            UpdateFall();
            UpdateMovement();
        }

        #region Movement
        [SerializeField] private float moveSpeed;
        private Vector3 inputDirection;
        private Vector3 lastPositiveInputDirection;
        private Axis projectedAxis;
        public Axis GetMovementAxese()
        {
            var camera = Camera.main.transform;
            var wallDirection = this.groundDirection;
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
            if (_inputDirection.magnitude > 0.1f)
            {
                lastPositiveInputDirection = _inputDirection.normalized;
            }
        }

        private void UpdateMovement()
        {
            if (!isGrounded) return;

            this.projectedAxis = GetMovementAxese();
            var moveDirection = Vector3.Normalize(this.projectedAxis.right * this.inputDirection.x + this.projectedAxis.up * this.inputDirection.y);
            this.behindDirection = -moveDirection;
            this.rigidbody.MovePosition(transform.position + moveSpeed * moveDirection);
            if (this.inputDirection.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection, transform.up);
            }
        }
        #endregion Movement

        #region Falling
        private Vector3 groundDirection;
        private Vector3 behindDirection;
        [SerializeField] private bool isGrounded = false;
        [SerializeField] private int numberOfContacts = 0;
        [SerializeField] private float raycastLength = 0.5f;
        private new Rigidbody rigidbody;
        private void UpdateFall()
        {
            if (isGrounded) return;

            RaycastHit hit;
            var direction = this.groundDirection;
            var position = transform.position + 0.5f * direction;
            Debug.DrawRay(position, this.raycastLength * direction, Color.red, 5f);
            var hasFoundNewGround = Physics.Raycast(position, direction, out hit, this.raycastLength, WHAT_IS_CLIMBABLE);
            if (hasFoundNewGround)
            {
                Debug.Log("Got it!");
                SetNewGround(hit.point, hit.normal);
                return;
            }

            position += this.raycastLength * direction;
            direction = this.behindDirection;
            Debug.DrawRay(position, this.raycastLength * direction, Color.green, 5f);
            hasFoundNewGround = Physics.Raycast(position, direction, out hit, this.raycastLength, WHAT_IS_CLIMBABLE);
            if (hasFoundNewGround)
            {
                Debug.Log("Got it!");
                SetNewGround(hit.point, hit.normal);
                return;
            }

            position += this.raycastLength * direction;
            direction = -this.groundDirection;
            Debug.DrawRay(position, this.raycastLength * direction, Color.blue, 5f);
            hasFoundNewGround = Physics.Raycast(position, direction, out hit, this.raycastLength, WHAT_IS_CLIMBABLE);
            if (hasFoundNewGround)
            {
                Debug.Log("Got it!");
                SetNewGround(hit.point, hit.normal);
                return;
            }

            position += this.raycastLength * direction;
            direction = -this.behindDirection;
            Debug.DrawRay(position, this.raycastLength * direction, Color.blue, 5f);
            hasFoundNewGround = Physics.Raycast(position, direction, out hit, this.raycastLength, WHAT_IS_CLIMBABLE);
            if (hasFoundNewGround)
            {
                Debug.Log("Got it!");

                SetNewGround(hit.point, hit.normal);
                return;
            }
        }
        

        private void SetNewGround(Vector3 _position, Vector3 _groundNormal)
        {
            Debug.DrawRay(_position, _groundNormal, Color.magenta, 5f);
            transform.position = _position + 0.5f * _groundNormal;
            isGrounded = true;
            this.groundDirection = -_groundNormal;
            transform.up = _groundNormal;
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
            if (((1 << _collision.gameObject.layer) & WHAT_IS_CLIMBABLE) == 0) return;

            ContactPoint[] contacts = _collision.contacts;
            Debug.Log("EXIT!");
            if (contacts.Length == 0)
            {
                isGrounded = false;
                Debug.Log("LetGo!");
                return;
            }
        }

        private void CheckCollisions(Collision _collision)
        {
            // Check if gameObject is climbable
            if (((1 << _collision.gameObject.layer) & WHAT_IS_CLIMBABLE) == 0) return;

            ContactPoint[] contacts = _collision.contacts;
            var contactCount = contacts.Length;
            foreach (var contact in contacts)
            {
                Debug.DrawRay(contact.point, contact.normal, Color.yellow, 3f);
            }
            if (numberOfContacts == contactCount) return;

            var contactPoint = contacts.FirstOrDefault(x => Vector3.Dot(x.normal,this.groundDirection) != -1f);
            Debug.Log("Change Contact! " + contactPoint.normal);
            if (contactPoint.normal == Vector3.zero) return;

            var collisionNormal = contactPoint.normal;
            SetNewGround(contactPoint.point, collisionNormal);
            this.projectedAxis = GetMovementAxese();
            var moveDirection = Vector3.Normalize(
                this.projectedAxis.right * this.lastPositiveInputDirection.x + this.projectedAxis.up * this.lastPositiveInputDirection.y
            );
            this.behindDirection = -moveDirection;
            this.rigidbody.MovePosition(transform.position + moveSpeed * moveDirection);
            numberOfContacts = contactCount;
        }
        #endregion Falling
    }
}
