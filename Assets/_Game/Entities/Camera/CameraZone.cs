using System;
using Creeper;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraZone : MonoBehaviour
{
    [Header("References")]
    public Collider effectZone;
    public Collider movementZone;
    public Transform arrowReference;

    [Space(10)]
    [Header("Settings")]
    public float minimumZoom;
    public float maximumZoom;
    
    public Quaternion rotation => arrowReference.rotation;

    public Bounds Bounds { get; private set; }

    private CameraController _cameraController;
    
    private void Awake()
    {
        _cameraController = FindObjectOfType<CameraController>();
        if (movementZone == null)
        {
            Bounds = effectZone.bounds;
        }
        else
        {
            Bounds = movementZone.bounds;
        }

        arrowReference.GetComponent<Renderer>().enabled = false;
    }

    public void SetActive()
    {
        _cameraController.AddCameraZone(this);
    }
    
    public void SetInactive()
    {
        _cameraController.RemoveCameraZone(this);
    }
}
