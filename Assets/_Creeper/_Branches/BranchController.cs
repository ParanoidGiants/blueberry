using UnityEngine;
using Creeper;

namespace Branches
{
    public class BranchController : MonoBehaviour
    {

        public float BranchAfter = 1f;
        public float BranchStrength = 1f;

        private float targetDeltaX = 0f;
        private float branchTime = 0f;
        private int branchesCount = 1;

        private LineRenderer branch;
        private HeadController headController;
        private Vector3 targetPosition;
        private float oldDeltaX = 0f;

        private void Start()
        {
            this.branch = GetComponent<LineRenderer>();
            this.branch.positionCount = 1;
            this.branch.SetPosition(branchesCount-1, transform.position);
            this.headController = FindObjectOfType<HeadController>();
            this.targetPosition = transform.position;
            AddBranch();
        }

        private void Update()
        {
            UpdateBranch();
        }

        private void UpdateBranch()
        {
            var directionWorldSpace = this.headController.MovementDirection;
            if (directionWorldSpace.magnitude < 0.1f) return;

            var targetDeltaX = Mathf.Lerp(this.oldDeltaX, this.targetDeltaX, this.branchTime / BranchAfter);
            this.targetPosition = transform.position + targetDeltaX * transform.parent.right;
            this.branch.SetPosition(this.branchesCount - 1, this.targetPosition);


            this.branchTime += Time.deltaTime;
            if (this.branchTime > BranchAfter)
            {
                AddBranch();
            }
        }

        public void AddBranch()
        {
            this.branchTime = 0f;
            this.branch.positionCount = ++branchesCount;
            var halfPi = Mathf.PI / 2f;
            var random = Random.Range(-halfPi, halfPi);
            this.oldDeltaX = this.targetDeltaX;
            this.targetDeltaX = Mathf.Sin(random) * BranchStrength;
        }
    }
}
