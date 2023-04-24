using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Creeper
{
    public class BranchController : MonoBehaviour
    {
        [SerializeField] private float _branchAfter = 1f;
        [SerializeField] private float _branchStrength = 1f;
        [SerializeField] private float _branchTime = 0f;
        [SerializeField] private int _currentSegmentIndex = -1;

        private float _targetDeltaX = 0f;

        private LineRenderer _line;
        private float _oldDeltaX = 0f;

        [SerializeField] private Branch _branch;
        [SerializeField] private Material _branchMaterial;
        [SerializeField] private Transform _head;
        private Vector2 _moveDirection;
        private bool _isInitialized;

        private void Init()
        {
            _line = GetComponent<LineRenderer>();
            _branch.initBaseMesh(_head.position, _head.up, _branchMaterial);
            var branchPosition = _head.position;
            _currentSegmentIndex = 1;
            _line.positionCount = 2;
            _line.SetPosition(0, branchPosition);
            _line.SetPosition(1, branchPosition);
            _branchTime = 0f;
        }

        private void Update()
        {

            if (_moveDirection.magnitude < 0.1f) return;

            if (!_isInitialized)
            {
                _isInitialized = true;
                Init();
            }

            UpdateBranch();
        }

        #region LineRenderer
        private void UpdateBranch()
        {
            //_branchTime += Time.deltaTime;
            //if (_branchTime >= _branchAfter)
            //{
            //    AddBranchNode();
            //}

            // Update Segment Position
            var factor = _branchTime / _branchAfter;
            var currentDeltaX = Mathf.Lerp(_oldDeltaX, _targetDeltaX, factor);
            var branchPosition = _head.position + currentDeltaX * _head.right;
            _line.SetPosition(_currentSegmentIndex, branchPosition);
            _branch.SetIvyNode(_currentSegmentIndex, branchPosition);
            _branch.UpdateBranchNodes();
        }

        public void SetMovementDirection(Vector2 direction)
        {
            _moveDirection = direction;
        }

        public void AddBranchNode()
        {
            var halfPi = Mathf.PI / 2f;
            var random = Random.Range(-halfPi, halfPi);
            _oldDeltaX = Mathf.Lerp(_oldDeltaX, _targetDeltaX, _branchTime / _branchAfter);
            var branchPosition = _head.position + _oldDeltaX * _head.right;
            _targetDeltaX = Mathf.Sin(random) * _branchStrength;
            _currentSegmentIndex++;
            _line.positionCount = _currentSegmentIndex + 1;
            _line.SetPosition(_currentSegmentIndex, branchPosition);
            _branch.AddIvyNode(branchPosition, Vector3.up);
            _branchTime = 0f;
        }
        #endregion LineRenderer
    }
}
