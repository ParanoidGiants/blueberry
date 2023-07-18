using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

namespace GameCamera
{
    public class CameraController : MonoBehaviour
    {
        private Creeper.HeadController _head;
        private List<CameraZone> _cameraZones;
        private bool _isDangling;
        private Transform _headTransform;
        private Transform _mainVCamTransform;

        [Space(10)]
        [Header("Settings")]
        [SerializeField] private float _moveSpeed = 1f;
        [SerializeField] private float _zoomSpeed = 10f;

        [SerializeField] private PlayableDirector _endDirector;
        public PlayableDirector EndDirector => _endDirector;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Time.timeScale = 1f;
            
            _cameraZones = new List<CameraZone>();
            
            _head = FindObjectOfType<Creeper.HeadController>();
            _headTransform = _head.transform;
            _mainVCamTransform = GetComponentInChildren<CinemachineVirtualCamera>().transform;
            transform.position = _headTransform.position;
        }

        private void LateUpdate()
        {
            if (_cameraZones.Count < 1) return;

            FollowTarget();
            Rotate();
            Zoom();
        }

        private void Zoom()
        {
            var currentLocalPosition = _mainVCamTransform.localPosition;
            _mainVCamTransform.localPosition = Vector3.Lerp(
                currentLocalPosition,
                -_cameraZones[^1].Zoom * Vector3.forward,
                Time.deltaTime * _zoomSpeed
            );
        }

        private void FollowTarget()
        {
            var targetPosition = _headTransform.position;
            if (_cameraZones.Count != 0)
            {
                var activeCameraZone = _cameraZones[^1];
                targetPosition = activeCameraZone.Bounds.ClosestPoint(targetPosition);
            }
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * _moveSpeed);
        }

        private void Rotate()
        {
            if (_cameraZones.Count != 0)
            {
                var activeCameraZone = _cameraZones[^1];
                transform.rotation = Quaternion.Lerp(transform.rotation, activeCameraZone.Rotation, Time.deltaTime * _moveSpeed);
                _head.UpdateMovementAxis();
                return;
            }
            
            _head.UpdateMovementAxis();
        }
        
        public void AddCameraZone(CameraZone cameraZone)
        {
            if (_isDangling)
            {
                _cameraZones.Clear();
                _isDangling = false;                
            }
            _cameraZones.Add(cameraZone);
        }
        
        public void RemoveCameraZone(CameraZone cameraZone)
        {
            if (_cameraZones.Count == 1)
            {
                _isDangling = true;
            }
            else
            {
                _cameraZones.Remove(cameraZone);
            }
        }
    }
}
