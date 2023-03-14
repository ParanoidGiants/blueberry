using System;
using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform Target;
    private Coroutine rotateRoutine;
    private Camera cam;
    public Camera Cam { get { return cam; } }
    public bool IsRotating;

    private void Start()
    {
        cam = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        FollowWithHandle();
    }

    void FollowWithHandle()
    {
        transform.position = Target.position;
    }

    void MoveOnXZPlane()
    {
        transform.position = new Vector3(Target.position.x, transform.position.y, Target.position.z);
    }
}
