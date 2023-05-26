using UnityEngine;

namespace Creeper
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform Target;
        [SerializeField] private float RotateSpeed = 0.1f;
        [SerializeField] private float MoveSpeed = 0.1f;

        private Vector3 _rotateDirection;
        private HeadController _head;


        private void Start()
        {
            _head = FindObjectOfType<HeadController>();
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

        private void Rotate()
        {
            if (_rotateDirection.magnitude == 0f) return;


            _head.UpdateGround();
            var rotateDirection = _rotateDirection * Time.deltaTime;
            transform.rotation *= Quaternion.Euler(rotateDirection.y, rotateDirection.z, -rotateDirection.x);
        }

        public void SetRotateDirection(Vector3 _direction)
        {
            _rotateDirection = RotateSpeed * _direction;
        }
    }
}
