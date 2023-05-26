using UnityEngine;

namespace Creeper
{
    public class BranchController : MonoBehaviour
    {
        [SerializeField] private RootsBranch _livelyBranch;
        [SerializeField] private Material _branchMaterial;
        [SerializeField] private Transform _head;
        [SerializeField] private float _branchAfter = 1f;
        [SerializeField] private float _branchStrength = 1f;
        [SerializeField] private float _branchTime = 0f;
        [SerializeField] private int _currentSegmentIndex = -1;

        private LineRenderer _line;
        private Vector2 _inputDirection;
        private Vector2 _lastInputDirection;
        public Vector2 InputDirection { set { _inputDirection = value; } }

        private float _targetDeltaX = 0f;
        private float _oldDeltaX = 0f;
        private bool _isInitialized;

        private void Start()
        {
            _currentSegmentIndex = 1;
            _livelyBranch.initBaseMesh(_head.position, _head.up, _branchMaterial);
            var branchPosition = _head.position;

            _line = GetComponent<LineRenderer>();
            var localScale = _head.localScale;
            _line.startWidth = localScale.x;
            _line.endWidth = localScale.x;
            _line.positionCount = 2;
            _line.SetPosition(0, branchPosition);
            _line.SetPosition(1, branchPosition);

            _branchTime = 0f;
        }

        private void Update()
        {
            if (_inputDirection.magnitude < 0.1f) return;
            UpdateBranch();
        }

        private void UpdateBranch()
        {
            _branchTime += Time.deltaTime;
            var isTurningAround = Vector3.Dot(_lastInputDirection, _inputDirection) < 0f;
            if (_branchTime >= _branchAfter || isTurningAround)
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

                _livelyBranch.AddIvyNode(oldBranchPosition, transform.forward, _head.localScale.x);
                _branchTime = 0f;
            }

            // Update Segment Position
            var factor = _branchTime / _branchAfter;
            var currentDeltaX = Mathf.Lerp(_oldDeltaX, _targetDeltaX, factor);
            var branchPosition = _head.position + currentDeltaX * _head.right;
            _line.SetPosition(_currentSegmentIndex, branchPosition);
            _livelyBranch.SetIvyNode(branchPosition);
            _livelyBranch.UpdateIvyNodes();
            _lastInputDirection = _inputDirection;
        }
    }
}
