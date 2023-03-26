using System;
using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform Target;
    public float RotateSpeed = 0.1f;
    public float MoveSpeed = 0.1f;
    private Camera cam;
    public Camera Cam { get { return cam; } }
    public bool IsRotating;
    private Vector3 rotateDirection;


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
        transform.position = Vector3.Lerp(transform.position, Target.position, Time.deltaTime * MoveSpeed);
    }

    private void MoveOnXZPlane()
    {
        transform.position = new Vector3(Target.position.x, transform.position.y, Target.position.z);
    }

    private void Rotate()
    {
        transform.rotation *= Quaternion.Euler(this.rotateDirection.y, this.rotateDirection.z, -this.rotateDirection.x); 
    }

    public void SetRotateDirection(Vector3 _direction)
    {
        this.rotateDirection = RotateSpeed * _direction;
    }
}
