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
        [Header("Zoom")]
        [SerializeField] private float ZoomSpeed = 0.1f;
        private float _zoomDirection;
        public List<CameraZone> _cameraZones;

        private void Start()
        {
            Application.targetFrameRate = 60;
            _head = FindObjectOfType<HeadController>();
            _cameraTransform = GetComponentInChildren<Camera>().transform;
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
            targetPositionZ = Mathf.Clamp(targetPositionZ, -_cameraZones[^1].maximumZoom, -_cameraZones[^1].minimumZoom);
            localPosition = new Vector3(localPosition.x, localPosition.y, targetPositionZ);
            _cameraTransform.localPosition = localPosition;
        }

        private void FollowTarget()
        {
            var targetPosition = Target.position;
            if (_cameraZones.Count != 0)
            {
                var activeCameraZone = _cameraZones[^1];
                targetPosition = activeCameraZone.Bounds.ClosestPoint(targetPosition);
            }
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * MoveSpeed);
        }

        private void Rotate()
        {
            if (_cameraZones.Count != 0)
            {
                var activeCameraZone = _cameraZones[^1];
                transform.rotation = Quaternion.Lerp(transform.rotation, activeCameraZone.rotation, Time.deltaTime * MoveSpeed);
                _head.UpdateMovementAxis();
                return;
            }
            
            _head.UpdateMovementAxis();
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
