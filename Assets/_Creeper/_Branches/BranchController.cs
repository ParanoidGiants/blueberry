using UnityEngine;
using Creeper;

namespace Branches
{
    public class BranchController : MonoBehaviour
    {
        [SerializeField] private float _branchAfter = 1f;
        [SerializeField] private float _branchStrength = 1f;
        [SerializeField] private float _branchTime = 0f;
        [SerializeField] private int _currentSegmentIndex = 0;

        private float _targetDeltaX = 0f;

        private LineRenderer _branch;
        private HeadController _headController;
        private Vector3 _branchPosition;
        private float _oldDeltaX = 0f;

        private void Start()
        {
            _branch = GetComponent<LineRenderer>();
            _branch.positionCount = 1;
            _branchPosition = transform.position;
            _branch.SetPosition(_currentSegmentIndex, _branchPosition);
            _headController = FindObjectOfType<HeadController>();
        }

        private void Update()
        {
            UpdateBranch();
        }

        private void UpdateBranch()
        {
            var directionWorldSpace = _headController.MovementDirection;
            if (directionWorldSpace.magnitude < 0.1f) return;

            _branchTime += Time.deltaTime;
            if (_branchTime >= _branchAfter)
            {
                _branchTime = 0f;
                AddBranch();
            }

            UpdateSegmentPosition();
        }

        private void UpdateSegmentPosition()
        {
            var currentDeltaX = Mathf.Lerp(_oldDeltaX, _targetDeltaX, _branchTime / _branchAfter);
            _branchPosition = transform.position + currentDeltaX * transform.parent.right;
            _branch.SetPosition(_currentSegmentIndex, _branchPosition);
        }

        public void AddBranch()
        {
            var halfPi = Mathf.PI / 2f;
            var random = Random.Range(-halfPi, halfPi);
            _oldDeltaX = Mathf.Lerp(_oldDeltaX, _targetDeltaX, _branchTime / _branchAfter);
            _targetDeltaX = Mathf.Sin(random) * _branchStrength;
            _currentSegmentIndex++;
            _branch.positionCount = _currentSegmentIndex + 1;
            UpdateSegmentPosition();
        }
    }
}
