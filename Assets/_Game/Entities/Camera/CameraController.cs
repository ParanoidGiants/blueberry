using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creeper
{
    public class CameraController : MonoBehaviour
    {
        private HeadController _head;
        public Transform Target;
        public Transform _cameraTransform;
        
        [Space(10)]
        [Header("Move")]
        [SerializeField] private float MoveSpeed = 0.1f;
        
        [Space(10)]
        [Header("Zoom")]
        [SerializeField] private float ZoomSpeed = 0.1f;
        private float _zoomDirection;
        public List<CameraZone> _cameraZones;
        public bool isDangling;
        
        private bool _isAnimating;
        private bool _isFlowerFocused;
        public bool IsFlowerFocused => _isFlowerFocused;
        

        private void Start()
        {
            Application.targetFrameRate = 60;
            _head = FindObjectOfType<HeadController>();
            _cameraZones = new List<CameraZone>();
        }

        private void LateUpdate()
        {
            if (_isAnimating)
            {
                return;
            }
            
            if (_cameraZones.Count < 1) return;

            FollowTarget();
            Rotate();
            Zoom();
        }

        private void Zoom()
        {
            var currentLocalPosition = _cameraTransform.localPosition;
            var nextLocalPosition = currentLocalPosition + Vector3.forward * (_zoomDirection * Time.deltaTime);
            // RaycastHit hit;
            // if (Physics.Raycast(
            //         transform.position,
            //         _cameraTransform.position - transform.position,
            //         out hit,
            //         _cameraZones[^1].maximumZoom,
            //         HeadController.WHAT_IS_CLIMBABLE
            //     )
            // ) {
            //     nextLocalPosition.z = Mathf.Max(hit.distance, nextLocalPosition.z);
            // }
            nextLocalPosition.z = Mathf.Clamp(
                nextLocalPosition.z, 
                -_cameraZones[^1].maximumZoom, 
                -_cameraZones[^1].minimumZoom
            );
            _cameraTransform.localPosition = Vector3.Lerp(
                currentLocalPosition, 
                nextLocalPosition,
                Time.deltaTime * ZoomSpeed
            );
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
            if (isDangling)
            {
                _cameraZones.Clear();
                isDangling = false;                
            }
            _cameraZones.Add(cameraZone);
        }
        
        public void RemoveCameraZone(CameraZone cameraZone)
        {
            if (_cameraZones.Count == 1)
            {
                isDangling = true;
            }
            else
            {
                _cameraZones.Remove(cameraZone);
            }
        }

        public void PlayEndAnimation()
        {
            
        }
        

        public void PlayStartAnimation()
        {
            
        }

        public IEnumerator MoveCameraToFlower(Vector3 targetPosition)
        {
            _isAnimating = true;
            var startRotation = transform.rotation;
            var targetRotation = Quaternion.Euler(20f, startRotation.eulerAngles.y, 0f);
            var startPosition = transform.position;
            
            var startZoom = _cameraTransform.localPosition.z;
            var targetZoom = -20f;
            
            var positionTime = 0f;
            while (positionTime <= 1f)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, positionTime);
                transform.rotation = Quaternion.Lerp(startRotation, targetRotation, positionTime);
                _cameraTransform.localPosition = new Vector3(0f, 0f, Mathf.Lerp(startZoom, targetZoom, positionTime));
                
                positionTime += Time.deltaTime;
                yield return null;
            }
            _isFlowerFocused = true;

            var rotationTime = 0f;
            while (_isAnimating)
            {
                transform.rotation = Quaternion.Euler(20f, rotationTime, 0f);
                rotationTime += Time.deltaTime * 20f;
                rotationTime %= 360f;
                yield return null;
            }
        }

        public void SetCameraActive(bool isActive)
        {
            _cameraTransform.gameObject.SetActive(isActive);
        }
    }
}
