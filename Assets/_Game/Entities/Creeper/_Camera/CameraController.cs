using UnityEngine;

namespace Creeper
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform Target;
        private HeadController _head;
        private Transform _cameraTransform;
        
        [Space(10)]
        [Header("Move")]
        [SerializeField] private float MoveSpeed = 0.1f;
        
        [Space(10)]
        [Header("Rotate")]
        [SerializeField] private float RotateSpeed = 0.1f;
        [SerializeField] private float MinPitch;
        [SerializeField] private float MaxPitch;
        private Vector3 _rotateDirection;
        private float _pitch = 0f;
        
        [Space(10)]
        [Header("Zoom")]
        [SerializeField] private float ZoomSpeed = 0.1f;
        [SerializeField] private float MinZoom = 3f;
        [SerializeField] private float MaxZoom = 15f;
        private float _zoomDirection;

        private void Start()
        {
            _head = FindObjectOfType<HeadController>();
            _cameraTransform = GetComponentInChildren<Camera>().transform;
            _pitch = transform.rotation.eulerAngles.x;
        }

        private void Update()
        {
            FollowTarget();
            Rotate();
            Zoom();
        }

        private void Zoom()
        {
            var localPosition = _cameraTransform.localPosition;
            var positionZ = localPosition.z;
            var targetPositionZ = positionZ + _zoomDirection * Time.deltaTime;
            targetPositionZ = Mathf.Clamp(targetPositionZ, -MaxZoom, -MinZoom);
            localPosition = new Vector3(localPosition.x, localPosition.y, targetPositionZ);
            _cameraTransform.localPosition = localPosition;
        }

        private void FollowTarget()
        {
            transform.position = Vector3.Lerp(transform.position, Target.position, Time.deltaTime * MoveSpeed);
        }

        private void Rotate()
        {
            if (_rotateDirection.magnitude == 0f) return;


            _head.UpdateGround();
            var rotateDirection = _rotateDirection * Time.deltaTime;
            
            // Rotate around world up axis
            transform.RotateAround(transform.position, Vector3.up, -rotateDirection.x);
            
            // Rotate around transform right axis
            _pitch += rotateDirection.y;
            // Prevent camera from going upside down
            _pitch = Mathf.Clamp(_pitch, MinPitch, MaxPitch);

            var rotation = transform.rotation;
            transform.rotation = Quaternion.Euler(_pitch, rotation.eulerAngles.y, rotation.eulerAngles.z);
        }

        public void SetRotateDirection(Vector3 _direction)
        {
            _rotateDirection = RotateSpeed * _direction;
        }

        public void SetZoomDirection(float direction)
        {
            _zoomDirection = ZoomSpeed * direction;
        }
    }
}
