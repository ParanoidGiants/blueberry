using UnityEngine;

namespace Creeper
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform Target;
        [SerializeField] private float RotateSpeed = 0.1f;
        [SerializeField] private float MoveSpeed = 0.1f;
        [SerializeField] private float MoveZSpeed = 1f;

        private Vector3 _rotateDirection;
        private HeadController _head;
        private Transform _cameraTransform;
        private LayerMask whatToIgnore;
        private float currentCameraZ = -10f;
        private float targetCameraZ = -10f;
        private float pitch = 0f;


        private void Start()
        {
            _head = FindObjectOfType<HeadController>();
            _cameraTransform = GetComponentInChildren<Camera>().transform;
            whatToIgnore = LayerMask.GetMask("Player");
            pitch = transform.rotation.eulerAngles.x;
        }

        private void Update()
        {
            FollowWithHandle();
            Rotate();
            

            RaycastHit hit;
            if (ShootRay(transform.position, -transform.forward, 10f, out hit, Color.red))
            {
                targetCameraZ = -hit.distance;
            }
            else
            {
                targetCameraZ = -10f;
            }
            
            currentCameraZ = Mathf.Lerp(currentCameraZ, targetCameraZ, Time.deltaTime * MoveZSpeed);
            _cameraTransform.localPosition = new Vector3(0, 0, currentCameraZ);
        }

        private void FollowWithHandle()
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
            pitch -= rotateDirection.y;
            pitch = Mathf.Clamp(pitch, -80, 80); // Prevent camera from going upside down

            var rotation = transform.rotation;
            transform.rotation = Quaternion.Euler(pitch, rotation.eulerAngles.y, rotation.eulerAngles.z);
        }

        public void SetRotateDirection(Vector3 _direction)
        {
            _rotateDirection = RotateSpeed * _direction;
        }

        private bool ShootRay(Vector3 rayOrigin, Vector3 rayDirection, float raycastLength, out RaycastHit hit, Color color)
        {
            Debug.DrawRay(rayOrigin, rayDirection * raycastLength, color);
            return Physics.Raycast(rayOrigin, rayDirection, out hit, raycastLength, ~whatToIgnore);
        }
    }
}
