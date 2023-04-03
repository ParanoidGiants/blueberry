using UnityEngine;

namespace Creeper
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private Transform Target;
        [SerializeField] private float RotateSpeed = 0.1f;
        [SerializeField] private float MoveSpeed = 0.1f;
        private Vector3 _rotateDirection;


        private void Start()
        {
            cam = GetComponentInChildren<Camera>();
        }

        private void Update()
        {
            FollowWithHandle();
            Rotate();
        }

        private void FollowWithHandle()
        {
            transform.position = Vector3.Lerp(transform.position, Target.position, Time.deltaTime * MoveSpeed);
        }

        private void MoveOnXZPlane()
        {
            transform.position = new Vector3(Target.position.x, transform.position.y, Target.position.z);
        }

        private void Rotate()
        {
            var rotateDirection = _rotateDirection * Time.deltaTime;
            transform.rotation *= Quaternion.Euler(rotateDirection.y, rotateDirection.z, -rotateDirection.x);
        }

        public void SetRotateDirection(Vector3 _direction)
        {
            _rotateDirection = RotateSpeed * _direction;
        }
    }
}
