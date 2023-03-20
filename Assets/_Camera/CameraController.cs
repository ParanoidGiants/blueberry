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
    private Vector2 rotateDirection;

    private void Start()
    {
        cam = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        FollowWithHandle();
        Rotate();
    }

    private void FollowWithHandle()
    {
        transform.position = Target.position;
    }

    private void MoveOnXZPlane()
    {
        transform.position = new Vector3(Target.position.x, transform.position.y, Target.position.z);
    }

    private void Rotate()
    {
        transform.rotation *= Quaternion.Euler(this.rotateDirection.y, 0f, -this.rotateDirection.x); 
    }

    public void SetRotateDirection(Vector2 _direction)
    {
        this.rotateDirection = _direction;
    }
}
