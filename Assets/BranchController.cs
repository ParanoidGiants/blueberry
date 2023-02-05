using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BranchController : MonoBehaviour
{
    private LineRenderer branch;

    public float BranchAfter = 1f;
    public float branchTime = 0f;
    private int NumberOfBranches = 1;
    public float TargetDeltaX = 0f;
    public float TargetDeltaZ = 0f;
    public Vector3 lastPosition;

    void Start()
    {
        branch = GetComponent<LineRenderer>();
        branch.positionCount = 1;
        branch.SetPosition(NumberOfBranches-1, transform.position);
        lastPosition = transform.position;
        AddBranch();
    }

    public void UpdateBranch()
    {
        branchTime += Time.deltaTime;

        if (branchTime > BranchAfter)
        {
            AddBranch();
        }
        var targetPosition = transform.position + TargetDeltaX * transform.right + TargetDeltaZ * transform.forward;
        var newPosition = Vector3.Lerp(lastPosition, targetPosition, branchTime / BranchAfter);
        branch.SetPosition(NumberOfBranches-1, newPosition);
    }

    private void AddBranch()
    {
        branchTime = 0f;
        lastPosition = branch.GetPosition(NumberOfBranches-1);
        branch.positionCount = ++NumberOfBranches;
        var random = Random.Range(0f, 2f * Mathf.PI);
        TargetDeltaX = Mathf.Sin(random);
        random = Random.Range(0f, 2f * Mathf.PI);
        TargetDeltaZ = Mathf.Sin(random);
    }
}
