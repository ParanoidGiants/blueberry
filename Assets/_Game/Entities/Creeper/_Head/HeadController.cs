using UnityEngine;
using RootMath;

namespace Creeper
{
    public class HeadController : MonoBehaviour
    {
        private static int _WHAT_IS_CLIMBABLE;
        public static int WHAT_IS_CLIMBABLE { get { return _WHAT_IS_CLIMBABLE; } }
        
        [Header("References")]
        [SerializeField] private BranchController Branch;
        private Rigidbody _rigidbody;
        private Transform _cameraTransform;
        
        [Header("Settings")]
        [SerializeField] private float _raycastLengthEpsilon;
        [SerializeField] private float _drawTime;
        [SerializeField] private float _moveSpeed;
        
        [Space(10)]
        [Header("Watchers")]
        [SerializeField] private bool _isGrounded = false;
        [SerializeField] private Vector3 _inputDirection;
        [SerializeField] private Vector3 _lastPosition;
        [SerializeField] private Vector3 _groundDirection;
        [SerializeField] private Vector3 _behindDirection;
        
        [Space(10)]
        [SerializeField] private Axis projectedAxis;
        [SerializeField] private ContactObjectManager contactObjectManager;
        
        
        public Vector3 InputDirection { set { _inputDirection = value; } }
        public Vector3 CalculateMovementDirection()
        { 
            return projectedAxis == null 
                ? Vector3.zero
                : Vector3.Normalize(projectedAxis.right * _inputDirection.x + projectedAxis.up * _inputDirection.y);
        }
        
        public Vector3 GetGroundPosition()
        {
            return transform.position + 0.5f * transform.localScale.z * _groundDirection;
        }
        
#region LifeCycle
        private void Start()
        {
            _WHAT_IS_CLIMBABLE = LayerMask.GetMask("Climbable");
            _rigidbody = GetComponent<Rigidbody>();
            _cameraTransform = Camera.main?.transform;
            contactObjectManager = new ContactObjectManager();
            _lastPosition = transform.position;
            _groundDirection = -transform.up;
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (!IsOfClimbableLayer(collision.gameObject.layer)) return;
            
            contactObjectManager.TryAddNormals(collision);
            UpdateGround();
        }
        
        private void OnCollisionStay(Collision collision)
        {
            if (!IsOfClimbableLayer(collision.gameObject.layer)) return;
            
            var isCollisionNew = contactObjectManager.TryAddNormals(collision);
            if (isCollisionNew)
            {
                UpdateGround();
            }
        }
        
        private void OnCollisionExit(Collision collision)
        {
            if (!IsOfClimbableLayer(collision.gameObject.layer)) return;
            
            contactObjectManager.RemoveContactObjects(collision.gameObject.GetInstanceID());
            UpdateGround();
        }
        
        private void FixedUpdate()
        {
            _rigidbody.velocity = Vector3.zero;
            
            _isGrounded = contactObjectManager.contactObjects.Count > 0;
            if (_isGrounded) UpdateMovement();
            else FindGround();
        }
#endregion LifeCycle
        
        private void UpdateMovement()
        {
            var moveDirection = CalculateMovementDirection();
            _behindDirection = -moveDirection;
            _lastPosition = transform.position;
            
            _rigidbody.MovePosition(transform.position + _moveSpeed * moveDirection);
            if (_inputDirection.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection, -_groundDirection);
            }
        }
        
        private void FindGround()
        {
            RaycastHit hit;
            // Search Below
            var rayOrigin = transform.position;
            var raycastLength = 0.5f * transform.localScale.z + _raycastLengthEpsilon;
            if (ShootRay(rayOrigin, _groundDirection, raycastLength, out hit, Color.red))
            {
                SetPosition(hit.point, hit.normal);
                return;
            }
            // Search Behind
            rayOrigin += raycastLength * _groundDirection;
            raycastLength = (transform.position - _lastPosition).magnitude + _raycastLengthEpsilon;
            if (ShootRay(rayOrigin, _behindDirection, raycastLength, out hit, Color.cyan))
            {
                SetPosition(hit.point, hit.normal);
                return;
            }

            // Search up
            rayOrigin += raycastLength * _behindDirection;
            raycastLength = _raycastLengthEpsilon;
            if (ShootRay(rayOrigin, -_groundDirection, raycastLength, out hit, Color.blue))
            {
                SetPosition(hit.point, hit.normal);
                return;
            }
            
            _rigidbody.MovePosition(_lastPosition);
        }

        private bool ShootRay(Vector3 rayOrigin, Vector3 rayDirection, float raycastLength, out RaycastHit hit, Color color)
        {
            Debug.DrawRay(rayOrigin, rayDirection * raycastLength, color, _drawTime);
            return Physics.Raycast(rayOrigin, rayDirection, out hit, raycastLength, WHAT_IS_CLIMBABLE);
        }
        
        private void SetPosition(Vector3 position, Vector3 normal)
        {
            var newPosition = position + normal * ((0.5f - _raycastLengthEpsilon) * transform.localScale.z);
            _rigidbody.MovePosition(newPosition);
        }
        
        public void UpdateGround()
        {
            contactObjectManager.UpdateGround();
            if (contactObjectManager.normal.magnitude == 0)
            {
                _isGrounded = false;
                return;
            }
            _isGrounded = true;
            _groundDirection = -contactObjectManager.normal;
            projectedAxis = CreateMovementAxis();
            transform.up = -_groundDirection;
            Branch.AddIvyNode();
            Debug.DrawRay(transform.position, transform.up, Color.green, 1f);
        }
        
        private Axis CreateMovementAxis()
        {
            var wallDirection = _groundDirection;
            var position = transform.position;
            var cameraPosition = _cameraTransform.position;
            return new Axis(
                position,
                cameraPosition,
                _cameraTransform.right,
                _cameraTransform.up,
                wallDirection
            );
        }

        private bool IsOfClimbableLayer(int layer)
        {
            var collayer = 1 << layer;
            var result = collayer & WHAT_IS_CLIMBABLE;
            return result != 0;
        }
    }
}