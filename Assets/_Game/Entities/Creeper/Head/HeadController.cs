using UnityEngine;

namespace Creeper
{
    public class HeadController : MonoBehaviour
    {
        private Rigidbody _rigidbody;
        private Transform _cameraTransform;
        
        [Header("References")]
        public MeshGenerator branchController;
        
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
        [SerializeField] private Utils.Axis projectedAxis;
        [SerializeField] private ContactObjectManager contactObjectManager;
        
        
        public Vector3 InputDirection { set { _inputDirection = value; } }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _cameraTransform = Camera.main.transform;

            var startLevel = FindObjectOfType<Level.StartLevel>();
            if (startLevel != null)
            {
                transform.position = startLevel.transform.position;
            }

            contactObjectManager = new ContactObjectManager();
            _lastPosition = transform.position;
            _groundDirection = -transform.up;
        }

        private void FixedUpdate()
        {
            _rigidbody.velocity = Vector3.zero;
            
            _isGrounded = contactObjectManager.HasContactObjects();
            if (_isGrounded) UpdateMovement();
            else FindGround();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!Utils.Helper.IsLayerClimbable(collision.gameObject.layer)) return;
            
            if (contactObjectManager.TryAddNormals(collision))
            {
                branchController.FixAllNodes();
                UpdateGround();
            }
        }
        
        private void OnCollisionStay(Collision collision)
        {
            if (!Utils.Helper.IsLayerClimbable(collision.gameObject.layer)) return;
            
            if (contactObjectManager.TryAddNormals(collision))
            {
                branchController.FixAllNodes();
                UpdateGround();
            }
        }
        
        private void OnCollisionExit(Collision collision)
        {
            if (!Utils.Helper.IsLayerClimbable(collision.gameObject.layer)) return;
            
            contactObjectManager.RemoveContactObjects(collision.gameObject.GetInstanceID());
            UpdateGround();
        }
        
        private void UpdateMovement()
        {
            var moveDirection = CalculateMovementDirection();
            _behindDirection = -moveDirection;
            _lastPosition = transform.position;
            
            _rigidbody.MovePosition(transform.position + Time.deltaTime * _moveSpeed * moveDirection);
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
            var raycastLength = transform.localScale.z + _raycastLengthEpsilon;
            if (ShootRay(rayOrigin, _groundDirection, raycastLength, out hit, Color.red))
            {
                SetPosition(hit.point, hit.normal);
                return;
            }
            // Search Behind
            rayOrigin += (0.5f * transform.localScale.z + _raycastLengthEpsilon) * _groundDirection;
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
            return Physics.Raycast(rayOrigin, rayDirection, out hit, raycastLength, Utils.Helper.WHAT_IS_CLIMBABLE);
        }
        
        private void SetPosition(Vector3 position, Vector3 normal)
        {
            var newPosition = position + normal * ((0.5f - _raycastLengthEpsilon) * transform.localScale.z);
            _rigidbody.MovePosition(newPosition);
        }
        
        private void UpdateGround()
        {
            contactObjectManager.UpdateGround();
            
            _isGrounded = contactObjectManager.AverageNormal.magnitude != 0; 
            if (!_isGrounded) return;
            
            _isGrounded = true;
            _groundDirection = -contactObjectManager.AverageNormal;
            UpdateMovementAxis();
            var transform1 = transform;
            transform1.up = -_groundDirection;
            Debug.DrawRay(transform1.position, transform1.up, Color.green, 1f);
        }

        public void UpdateMovementAxis()
        {
            projectedAxis = Utils.Helper.CreateMovementAxis(transform, _cameraTransform, _groundDirection);
        }
        
        public Vector3 CalculateMovementDirection()
        { 
            return projectedAxis == null 
                ? Vector3.zero
                : Vector3.Normalize(projectedAxis.right * _inputDirection.x + projectedAxis.up * _inputDirection.y);
        }
    }
}
