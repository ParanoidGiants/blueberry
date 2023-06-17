using System.Collections.Generic;
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
        public List<CameraZone> _cameraZones;

        private void Start()
        {
            _head = FindObjectOfType<HeadController>();
            _cameraTransform = GetComponentInChildren<Camera>().transform;
            _pitch = transform.rotation.eulerAngles.x;
            _cameraZones = new List<CameraZone>();
        }

        private void LateUpdate()
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
            var targetPosition = Target.position;
            if (_cameraZones.Count != 0)
            {
                // for c# noobs: ^1 means last element in array
                var activeCameraZone = _cameraZones[^1];
                
                if (activeCameraZone.FixPosition)
                {
                    targetPosition = activeCameraZone.position;
                }
                else
                {
                    targetPosition = activeCameraZone.Bounds.ClosestPoint(targetPosition);
                }
            }
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * MoveSpeed);
        }

        private void Rotate()
        {
            if (_cameraZones.Count != 0 && _cameraZones[^1].FixRotation)
            {
                var activeCameraZone = _cameraZones[^1];
                transform.rotation = Quaternion.Lerp(transform.rotation, activeCameraZone.rotation, Time.deltaTime * MoveSpeed);
                _head.UpdateMovementAxis();
                return;
            }
            
            if (_rotateDirection.magnitude == 0f) return;

            var rotateDirection = _rotateDirection * Time.deltaTime;
            
            // Rotate around world up axis
            transform.RotateAround(transform.position, Vector3.up, -rotateDirection.x);
            
            // Rotate around transform right axis
            _pitch += rotateDirection.y;
            // Prevent camera from going upside down
            _pitch = Mathf.Clamp(_pitch, MinPitch, MaxPitch);

            var rotation = transform.rotation;
            transform.rotation = Quaternion.Euler(_pitch, rotation.eulerAngles.y, rotation.eulerAngles.z);
            _head.UpdateMovementAxis();
        }

        public void SetRotateDirection(Vector3 _direction)
        {
            _rotateDirection = RotateSpeed * _direction;
        }

        public void SetZoomDirection(float direction)
        {
            _zoomDirection = ZoomSpeed * direction;
        }
        
        public void AddCameraZone(CameraZone cameraZone)
        {
            _cameraZones.Add(cameraZone);
        }
        
        public void RemoveCameraZone(CameraZone cameraZone)
        {
            _cameraZones.Remove(cameraZone);
        }
    }
}
