using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RootMath;

namespace Creeper
{
    public class HeadController : MonoBehaviour
    {
        #region Essentials
        private static int WHAT_IS_CLIMBABLE;
        private void Start()
        {
            WHAT_IS_CLIMBABLE = LayerMask.GetMask("Climbable");
            this.rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            this.rigidbody.velocity = Vector3.zero;
            DebugDrawNormal();

            if (!_isGrounded)
            {
                var hasFoundGround = false;
                for (int i = -3; i < 0 && !hasFoundGround; i++)
                {
                    var raycastLength = Mathf.Pow(2f, i);
                    hasFoundGround = FindGround(raycastLength);
                }
            }

            UpdateMovement();
        }

        private void DebugDrawNormal()
        {
            var currentNormal = CurrentNormals.FirstOrDefault(x => x.GameObjectId == _currentObjectIndex);
            if (_isGrounded && currentNormal != null)
            {
                Debug.DrawRay(transform.position, currentNormal.Normal, Color.green);
            }
        }
        #endregion Essentials

        #region Movement
        [SerializeField] private float moveSpeed;
        private Vector3 inputDirection;
        public Vector3 MovementDirection { 
            get {
                return projectedAxis == null 
                    ? Vector3.zero
                    : Vector3.Normalize(this.projectedAxis.right * this.inputDirection.x + this.projectedAxis.up * this.inputDirection.y);
            }
        }
        private Axis projectedAxis;
        public Axis ProjectedAxis { get { return projectedAxis; } }

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

        public void SetMovementDirection(Vector3 _inputDirection)
        {
            this.inputDirection = _inputDirection;
        }

        private void UpdateMovement()
        {
            if (!_isGrounded) return;

            var moveDirection = MovementDirection;
            _behindDirection = -moveDirection;
            this.rigidbody.MovePosition(transform.position + moveSpeed * moveDirection);
            if (this.inputDirection.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection, transform.up);
            }
        }
        #endregion Movement

        #region Climbing
        private Vector3 _groundDirection;
        private Vector3 _behindDirection;
        [SerializeField] private bool _isGrounded = false;
        [SerializeField] private float _raycastLength = 0.5f;
        [SerializeField] private int _currentObjectIndex = -1;
        private new Rigidbody rigidbody;
        private bool FindGround(float raycastLength)
        {
            RaycastHit hit;
            var direction = _groundDirection;
            var position = transform.position + 0.5f * transform.localScale.z * direction;
            Debug.DrawRay(position, raycastLength * direction, Color.red, 5f);
            var hasFoundNewGround = Physics.Raycast(position, direction, out hit, raycastLength, WHAT_IS_CLIMBABLE);
            if (hasFoundNewGround)
            {
                SetNewGround(hit.point, hit.normal);
                return true;
            }

            position += raycastLength * direction;
            direction = _behindDirection;
            Debug.DrawRay(position, raycastLength * direction, Color.green, 5f);
            hasFoundNewGround = Physics.Raycast(position, direction, out hit, raycastLength, WHAT_IS_CLIMBABLE);
            if (hasFoundNewGround)
            {
                SetNewGround(hit.point, hit.normal);
                return true;
            }

            position += raycastLength * direction;
            direction = -_groundDirection;
            Debug.DrawRay(position, raycastLength * direction, Color.blue, 5f);
            hasFoundNewGround = Physics.Raycast(position, direction, out hit, raycastLength, WHAT_IS_CLIMBABLE);
            if (hasFoundNewGround)
            {
                SetNewGround(hit.point, hit.normal);
                return true;
            }

            position += raycastLength * direction;
            direction = -_behindDirection;
            Debug.DrawRay(position, raycastLength * direction, Color.blue, 5f);
            hasFoundNewGround = Physics.Raycast(position, direction, out hit, raycastLength, WHAT_IS_CLIMBABLE);
            if (hasFoundNewGround)
            {
                SetNewGround(hit.point, hit.normal);
                return true;
            }
            return false;
        }

        private void SetNewGround(Vector3 _position, Vector3 _groundNormal)
        {
            Debug.DrawRay(_position, _groundNormal, Color.magenta, 1f);
            _isGrounded = true;
            _groundDirection = -_groundNormal;
            transform.position = _position + 0.5f * transform.localScale.z * _groundNormal;
            transform.up = _groundNormal;
        }

        private void SetNewGround(Vector3 _groundNormal)
        {
            Debug.DrawRay(transform.position, _groundNormal, Color.gray, 1f);
            _isGrounded = true;
            _groundDirection = -_groundNormal;
            this.projectedAxis = CreateMovementAxis();
            transform.up = _groundNormal;
        }
        
        public List<ContactNormal> CurrentNormals = new List<ContactNormal>();
        private void OnCollisionEnter(Collision _collision)
        {
            var normal = _collision.GetContact(_collision.contactCount - 1).normal;
            var instanceId = _collision.gameObject.GetInstanceID();
            CurrentNormals.Add(new ContactNormal(instanceId, normal));
            _currentObjectIndex = instanceId;
            SetNewGround(normal);
        }

        private void OnCollisionStay(Collision _collision)
        {
            var normal = _collision.GetContact(_collision.contactCount-1).normal;
            var instanceId = _collision.gameObject.GetInstanceID();

            var currentNormal = CurrentNormals.FirstOrDefault(x => x.GameObjectId == instanceId);
            var currentNewNormals = new List<ContactNormal>();
            if (currentNormal == null)
            {
                var contactNormal = new ContactNormal(instanceId, normal);
                CurrentNormals.Add(contactNormal);
                currentNewNormals.Add(contactNormal);
            }
            else if (!RMath.AreDirectíonsConsideredEqual(currentNormal.Normal, normal))
            {
                currentNormal.Normal = normal;
                _currentObjectIndex = instanceId;
                SetNewGround(currentNormal.Normal);
            }
        }

        private void OnCollisionExit(Collision _collision)
        {
            var instanceId = _collision.gameObject.GetInstanceID();
            var normal = CurrentNormals.FirstOrDefault(x => x.GameObjectId == instanceId);
            CurrentNormals.Remove(normal);
            _isGrounded = CurrentNormals.Count != 0;

            if (_isGrounded)
            {
                SetNewGround(CurrentNormals[CurrentNormals.Count - 1].Normal);
            }
        }
        #endregion Climbing
    }
}
