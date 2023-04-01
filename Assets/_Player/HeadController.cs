using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RootMath;
using System.Collections;

namespace Creeper
{
    [Serializable]
    public class CNormal
    {
        public int GameObjectId;
        public Vector3 Normal;

        public CNormal(int _gameObjectId, Vector3 _normal)
        {
            GameObjectId = _gameObjectId;
            Normal = _normal;
        }
    }

    public class HeadController : MonoBehaviour
    {
        public static int WHAT_IS_CLIMBABLE;
        private void Start()
        {
            WHAT_IS_CLIMBABLE = LayerMask.GetMask("Climbable");
            this.rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            this.rigidbody.velocity = Vector3.zero;
            DebugLog();
            UpdateFall();
            UpdateMovement();
        }

        #region Movement
        [SerializeField] private float moveSpeed;
        private Vector3 inputDirection;
        private Axis projectedAxis;
        public Axis GetMovementAxese()
        {
            var camera = Camera.main.transform;
            var wallDirection = this.groundDirection;
            var axis = new Axis()
            {
                right = ChatGPT.IntersectingLine(transform.position, camera.right, camera.position, wallDirection),
                up = ChatGPT.IntersectingLine(transform.position, camera.up, camera.position, wallDirection)
            };
            return axis;
        }

        public void SetMovementDirection(Vector3 _inputDirection)
        {
            this.inputDirection = _inputDirection;
        }

        private void UpdateMovement()
        {
            if (!isGrounded) return;

            this.projectedAxis = GetMovementAxese();
            var moveDirection = Vector3.Normalize(this.projectedAxis.right * this.inputDirection.x + this.projectedAxis.up * this.inputDirection.y);
            this.behindDirection = -moveDirection;
            this.rigidbody.MovePosition(transform.position + moveSpeed * moveDirection);
            if (this.inputDirection.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection, transform.up);
            }
        }
        #endregion Movement

        #region Falling
        private Vector3 groundDirection;
        private Vector3 behindDirection;
        [SerializeField] private bool isGrounded = false;
        [SerializeField] private float raycastLength = 0.5f;
        private new Rigidbody rigidbody;
        private void UpdateFall()
        {
            if (isGrounded) return;

            RaycastHit hit;
            var direction = this.groundDirection;
            var position = transform.position + 0.5f * transform.localScale.z * direction;
            Debug.DrawRay(position, this.raycastLength * direction, Color.red, 5f);
            var hasFoundNewGround = Physics.Raycast(position, direction, out hit, this.raycastLength, WHAT_IS_CLIMBABLE);
            if (hasFoundNewGround)
            {
                Debug.Log("Got it!");
                SetNewGround(hit.point, hit.normal);
                return;
            }

            position += this.raycastLength * direction;
            direction = this.behindDirection;
            Debug.DrawRay(position, this.raycastLength * direction, Color.green, 5f);
            hasFoundNewGround = Physics.Raycast(position, direction, out hit, this.raycastLength, WHAT_IS_CLIMBABLE);
            if (hasFoundNewGround)
            {
                Debug.Log("Got it!");
                SetNewGround(hit.point, hit.normal);
                return;
            }

            position += this.raycastLength * direction;
            direction = -this.groundDirection;
            Debug.DrawRay(position, this.raycastLength * direction, Color.blue, 5f);
            hasFoundNewGround = Physics.Raycast(position, direction, out hit, this.raycastLength, WHAT_IS_CLIMBABLE);
            if (hasFoundNewGround)
            {
                Debug.Log("Got it!");
                SetNewGround(hit.point, hit.normal);
                return;
            }

            position += this.raycastLength * direction;
            direction = -this.behindDirection;
            Debug.DrawRay(position, this.raycastLength * direction, Color.blue, 5f);
            hasFoundNewGround = Physics.Raycast(position, direction, out hit, this.raycastLength, WHAT_IS_CLIMBABLE);
            if (hasFoundNewGround)
            {
                Debug.Log("Got it!");
                SetNewGround(hit.point, hit.normal);
                return;
            }
        }

        private void SetNewGround(Vector3 _position, Vector3 _groundNormal)
        {
            Debug.DrawRay(_position, _groundNormal, Color.magenta, 1f);
            this.isGrounded = true;
            this.groundDirection = -_groundNormal;
            transform.position = _position + 0.5f * transform.localScale.z * _groundNormal;
            transform.up = _groundNormal;
        }

        private void SetNewGround(Vector3 _groundNormal)
        {
            Debug.DrawRay(transform.position, _groundNormal, Color.gray, 1f);
            this.isGrounded = true;
            this.groundDirection = -_groundNormal;
            transform.up = _groundNormal;
        }

        public int CurrentObjectIndex = -1;
        
        public List<CNormal> CurrentNormals = new List<CNormal>();
        private void OnCollisionEnter(Collision _collision)
        {
            var normal = _collision.GetContact(_collision.contactCount - 1).normal;
            var instanceId = _collision.gameObject.GetInstanceID();
            CurrentNormals.Add(new CNormal(instanceId, normal));
            CurrentObjectIndex = instanceId;
            SetNewGround(normal);
        }

        private void OnCollisionStay(Collision _collision)
        {
            var normal = _collision.GetContact(_collision.contactCount-1).normal;
            var instanceId = _collision.gameObject.GetInstanceID();

            var currentNormal = CurrentNormals.FirstOrDefault(x => x.GameObjectId == instanceId);
            if (currentNormal == null)
            {
                CurrentNormals.Add(new CNormal(instanceId, normal));
            }
            else if (!IsConsideredEqual(currentNormal.Normal, normal))
            {
                currentNormal.Normal = normal;
                CurrentObjectIndex = instanceId;
                SetNewGround(currentNormal.Normal);
            }
        }

        private void OnCollisionExit(Collision _collision)
        {
            var instanceId = _collision.gameObject.GetInstanceID();
            var normal = CurrentNormals.FirstOrDefault(x => x.GameObjectId == instanceId);
            CurrentNormals.Remove(normal);
            isGrounded = CurrentNormals.Count != 0;

            if (isGrounded)
            {
                SetNewGround(CurrentNormals[CurrentNormals.Count - 1].Normal);
            }
        }

        private bool IsConsideredEqual(Vector3 _direction1, Vector3 _direction2)
        {
            return Vector3.Dot(_direction1, _direction2) > 0.99f;
        }

        private void DebugLog()
        {
            var currentNormal = CurrentNormals.FirstOrDefault(x => x.GameObjectId == CurrentObjectIndex);
            if (isGrounded && currentNormal != null)
            {
                Debug.DrawRay(transform.position, currentNormal.Normal, Color.green);
            }
        }

        #endregion Falling
    }
}
