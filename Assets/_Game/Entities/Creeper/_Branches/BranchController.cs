using System;
using UnityEngine;

namespace Creeper
{
    public class BranchController : MonoBehaviour
    {
        [SerializeField] private Transform head;
        private int _currentSegmentIndex = -1;
        
        public float branchLength = 2f;
        public int segmentsMaxCount = 15;
        
        private Vector3 _lastPosition;
        private LineRenderer _line;
        private Vector2 _inputDirection;
        public Vector2 InputDirection { set { _inputDirection = value; } }
        private bool _isInitialized;

        private void Start()
        {
            _currentSegmentIndex = 1;
            var position = head.position;
            // _livelyBranch.initBaseMesh(position, _head.up, _branchMaterial);
            _lastPosition = position;

            _line = GetComponent<LineRenderer>();
            var localScale = head.localScale;
            _line.startWidth = localScale.x;
            _line.endWidth = localScale.x;
            _line.positionCount = 2;
            _line.SetPosition(0, position);
            _line.SetPosition(1, position);
        }

        private void Update()
        {
            if (_inputDirection.magnitude < 0.1f) return;
            UpdateBranch();
        }

        private void UpdateBranch()
        {
            var position = head.position;
            var distance = Vector3.Distance(_lastPosition, position);
            if (distance > branchLength)
            {
                AddNode(position);
            }
            
            var moveDirection = position - _line.GetPosition(_currentSegmentIndex);
            var linePositions = new Vector3[_currentSegmentIndex+1];
            _line.GetPositions(linePositions);
            var start = Mathf.Clamp(_currentSegmentIndex - segmentsMaxCount, 0, _currentSegmentIndex);
            for (var i = start; i <= _currentSegmentIndex; i++)
            {
                var moveFactor =  (float) (i - start) / segmentsMaxCount;
                var linePosition = linePositions[i] + moveDirection * moveFactor;
                _line.SetPosition(i, linePosition);
            }
        }

        public void AddNode(Vector3 position)
        {
            _currentSegmentIndex++;
            _line.positionCount = _currentSegmentIndex+1;
            _lastPosition = position;
            _line.SetPosition(_currentSegmentIndex, position);
        }
    }
}
