using UnityEngine;

namespace Creeper
{
    public class BranchController : MonoBehaviour
    {
        [SerializeField] private Branch _deadBranch;
        [SerializeField] private Branch _livelyBranch;
        [SerializeField] private Material _branchMaterial;
        [SerializeField] private Transform _head;
        [SerializeField] private float _branchAfter = 1f;
        [SerializeField] private float _branchStrength = 1f;
        [SerializeField] private float _branchTime = 0f;
        [SerializeField] private int _currentSegmentIndex = -1;

        private LineRenderer _line;
        private Vector2 _moveDirection;
        public Vector2 InputDirection { set { _moveDirection = value; } }

        private float _targetDeltaX = 0f;
        private float _oldDeltaX = 0f;
        private bool _isInitialized;

        private void Init()
        {
            _currentSegmentIndex = 1;
            _livelyBranch.initBaseMesh(_head.position, _head.up, _branchMaterial);
            var branchPosition = _head.position;

            _line = GetComponent<LineRenderer>();
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

        private void UpdateBranch()
        {
            _branchTime += Time.deltaTime;
            if (_branchTime >= _branchAfter)
            {
                // Add Ivy Node
                var halfPi = Mathf.PI / 2f;
                var random = Random.Range(-halfPi, halfPi);
                _oldDeltaX = Mathf.Lerp(_oldDeltaX, _targetDeltaX, _branchTime / _branchAfter);
                var oldBranchPosition = _head.position + _oldDeltaX * _head.right;
                _targetDeltaX = Mathf.Sin(random) * _branchStrength;
                _currentSegmentIndex++;
                _line.positionCount = _currentSegmentIndex + 1;
                _line.SetPosition(_currentSegmentIndex, oldBranchPosition);

                _livelyBranch.AddIvyNode(oldBranchPosition, transform.forward);
                _branchTime = 0f;
            }

            // Update Segment Position
            var factor = _branchTime / _branchAfter;
            var currentDeltaX = Mathf.Lerp(_oldDeltaX, _targetDeltaX, factor);
            var branchPosition = _head.position + currentDeltaX * _head.right;
            _line.SetPosition(_currentSegmentIndex, branchPosition);
            _livelyBranch.SetIvyNode(branchPosition);
            _livelyBranch.UpdateIvyNodes();
        }
    }
}
