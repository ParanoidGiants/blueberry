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
        private Vector3 _branchPosition;
        private float _oldDeltaX = 0f;

        [SerializeField] private Branch _branch;
        [SerializeField] private float branchRadius;
        [SerializeField] private Material _branchMaterial;
        private Vector2 _moveDirection;

        private void Start()
        {
            _line = GetComponent<LineRenderer>();
            _branchPosition = transform.position;
            _branch.initEmptyMesh(branchRadius, _branchMaterial);
            AddBranch();
        }

        private void Update()
        {
            if (_moveDirection.magnitude < 0.1f) return;


            UpdateBranch();
            _branch.UpdateBranch();
        }

        #region LineRenderer
        private void UpdateBranch()
        {
            _branchTime += Time.deltaTime;
            if (_branchTime >= _branchAfter)
            {
                AddBranch();
            }

            // Update Segment Position
            var factor = _branchTime / _branchAfter;
            var currentDeltaX = Mathf.Lerp(_oldDeltaX, _targetDeltaX, factor);
            _branchPosition = transform.position + currentDeltaX * transform.parent.right;
            _line.SetPosition(_currentSegmentIndex, _branchPosition);
            _branch.SetIvyNode(_currentSegmentIndex, _branchPosition, factor);
            _branch.UpdateBranch();
        }

        public void SetMovementDirection(Vector2 direction)
        {
            _moveDirection = direction;
        }

        public void AddBranch()
        {
            _currentSegmentIndex++;
            _branchTime = 0f;
            var halfPi = Mathf.PI / 2f;
            var random = Random.Range(-halfPi, halfPi);
            _oldDeltaX = Mathf.Lerp(_oldDeltaX, _targetDeltaX, _branchTime / _branchAfter);
            _targetDeltaX = Mathf.Sin(random) * _branchStrength;
            _line.positionCount = _currentSegmentIndex + 1;
            _line.SetPosition(_currentSegmentIndex, _branchPosition);
            _branch.AddIvyNode(_branchPosition, Vector3.up);
        }
        #endregion LineRenderer
    }
}
