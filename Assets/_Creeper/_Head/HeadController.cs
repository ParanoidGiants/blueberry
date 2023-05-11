using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RootMath;

namespace Creeper
{
    public class HeadController : MonoBehaviour
    {
        #region Essentials
        private static int _WHAT_IS_CLIMBABLE;
        public static int WHAT_IS_CLIMBABLE { get { return _WHAT_IS_CLIMBABLE; } }
        private void Start()
        {
            _WHAT_IS_CLIMBABLE = LayerMask.GetMask("Climbable");
            rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
        }

        private void FixedUpdate()
        {
            rigidbody.velocity = Vector3.zero;
            DebugDrawNormal();

            if (!_isGrounded) FindGround();
            else UpdateMovement();
        }

        private void DebugDrawNormal()
        {
            var currentNormal = -_groundDirection;
            if (_isGrounded)
            {
                Debug.DrawRay(transform.position, currentNormal, Color.green);
            }
        }
        #endregion Essentials

        #region Movement
        [SerializeField] private float moveSpeed;
        private Vector3 inputDirection;
        public Vector3 InputDirection { set { this.inputDirection = value; } }
        public Vector3 MovementDirection { 
            get {
                return _projectedAxis == null 
                    ? Vector3.zero
                    : Vector3.Normalize(this._projectedAxis.right * this.inputDirection.x + this._projectedAxis.up * this.inputDirection.y);
            }
        }

        private Axis _projectedAxis;

        private Axis CreateMovementAxis()
        {
            var camera = Camera.main.transform;
            var wallDirection = _groundDirection;
            return new Axis()
            {
                right = ChatGPT.IntersectingLine(transform.position, camera.right, camera.position, wallDirection),
                up = ChatGPT.IntersectingLine(transform.position, camera.up, camera.position, wallDirection)
            };
        }


        private void UpdateMovement()
        {
            var moveDirection = MovementDirection;
            _behindDirection = -moveDirection;
            rigidbody.MovePosition(transform.position + moveSpeed * moveDirection);
            if (this.inputDirection.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection, transform.up);
            }
        }

        public void UpdateAxis()
        {
            _projectedAxis = CreateMovementAxis();
        }
        #endregion Movement

        #region Climbing
        private Vector3 _groundDirection;
        private Vector3 _behindDirection;
        [SerializeField] private bool _isGrounded = false;
        [SerializeField] private int _currentObjectIndex = -1;
        private new Rigidbody rigidbody;


        [SerializeField] private float _raycastLength;
        [SerializeField] private float _raycastLengthDown;
        [SerializeField] private float _drawTime;
        private void FindGround()
        {
            var raycastLength = _raycastLength;

            RaycastHit hit;
            var rayDirection = _groundDirection;
            
            var rayOrigin = transform.position + 0.5f * transform.localScale.z * rayDirection;
            Debug.DrawRay(rayOrigin, _raycastLengthDown * rayDirection, Color.red, _drawTime);
            var hasFoundNewGround = Physics.Raycast(rayOrigin, rayDirection, out hit, _raycastLengthDown, WHAT_IS_CLIMBABLE);
            if (hasFoundNewGround)
            {
                SetPositionToHitPoint(hit.point, hit.normal);
                return;
            }

            for (int i = -5; i < 5; i++)
            {
                var rayFrom = rayOrigin + _raycastLengthDown * _groundDirection;

                raycastLength = _raycastLength * Mathf.Pow(2f, i);
                rayDirection = _behindDirection;
                Debug.DrawRay(rayFrom, raycastLength * rayDirection, Color.green, _drawTime);
                hasFoundNewGround = Physics.Raycast(rayFrom, rayDirection, out hit, raycastLength, WHAT_IS_CLIMBABLE);
                if (hasFoundNewGround)
                {
                    SetPositionToHitPoint(hit.point, hit.normal);
                    return;
                }

                rayFrom += raycastLength * rayDirection;
                rayDirection = -_groundDirection;
                Debug.DrawRay(rayFrom, raycastLength * rayDirection, Color.blue, _drawTime);
                hasFoundNewGround = Physics.Raycast(rayFrom, rayDirection, out hit, raycastLength, WHAT_IS_CLIMBABLE);
                if (hasFoundNewGround)
                {
                    SetPositionToHitPoint(hit.point, hit.normal);
                    return;
                }

                rayFrom += raycastLength * rayDirection;
                rayDirection = -_behindDirection;
                Debug.DrawRay(rayFrom, raycastLength * rayDirection, Color.blue, _drawTime);
                hasFoundNewGround = Physics.Raycast(rayFrom, rayDirection, out hit, raycastLength, WHAT_IS_CLIMBABLE);
                if (hasFoundNewGround)
                {
                    SetPositionToHitPoint(hit.point, hit.normal);
                    return;
                }
            }
        }
        private void SetPositionToHitPoint(Vector3 _position, Vector3 _groundNormal)
        {
            Debug.DrawRay(_position, _groundNormal, Color.magenta, 1f);
            transform.position = _position + 0.5f * transform.localScale.z * _groundNormal;
            transform.up = _groundNormal;
        }

        public Vector3 GroundPosition { get { return transform.position - 0.5f * transform.up; } }

        private void RecalculateGroundDirection()
        {
            var newGroundDirection = Vector3.zero;
            foreach (var contactNormal in CurrentContactNormals)
            {
                newGroundDirection += contactNormal.Normal;
            }

            if (newGroundDirection.magnitude == 0f)
            {
                int index = Random.Range(0, CurrentContactNormals.Count);
                newGroundDirection = CurrentContactNormals[index].Normal;
            }
            else
            {
                newGroundDirection.Normalize();
            }

            Debug.DrawRay(transform.position, newGroundDirection, Color.gray, 1f);
            _isGrounded = true;
            _groundDirection = -newGroundDirection;
            UpdateAxis();
            transform.up = newGroundDirection;
        }
        
        public List<ContactNormal> CurrentContactNormals = new List<ContactNormal>();
        private void OnCollisionEnter(Collision collision)
        {
            var normal = collision.GetContact(collision.contactCount - 1).normal;
            var instanceId = collision.gameObject.GetInstanceID();
            CurrentContactNormals.Add(new ContactNormal(instanceId, normal));
            _currentObjectIndex = instanceId;
            RecalculateGroundDirection();
        }

        private void OnCollisionStay(Collision collision)
        {
            var contactCount = collision.contactCount;
            var contacts = new ContactPoint[contactCount];
            collision.GetContacts(contacts);
            var collisionInstanceId = collision.gameObject.GetInstanceID();
            var newContacts = contacts.Where(x => IsNewContact(collisionInstanceId, x.normal)).ToArray();
            if (newContacts.Length == 0) return;

            foreach (var contact in newContacts)
            {
                CurrentContactNormals.Add(new ContactNormal(collisionInstanceId, contact.normal));
            }

            
            RecalculateGroundDirection();
        }

        private bool IsNewContact(int collisionInstanceId, Vector3 collisionNormal)
        {
            return !CurrentContactNormals.Any(x => 
                x.GameObjectId == collisionInstanceId
                && RMath.AreDirectionsConsideredEqual(x.Normal, collisionNormal)
            );
        }
        private void OnCollisionExit(Collision collision)
        {
            CurrentContactNormals.RemoveAll(x => x.GameObjectId == collision.gameObject.GetInstanceID());
            _isGrounded = CurrentContactNormals.Count != 0;

            if (_isGrounded)
            {
                RecalculateGroundDirection();
            }
        }
        #endregion Climbing
    }
}
