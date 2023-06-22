using System;
using Creeper;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraZone : MonoBehaviour
{
    [Header("References")]
    public Collider movementZone;
    [SerializeField] private Transform arrowReference;

    [FormerlySerializedAs("_minimumZoom")]
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
        Bounds = movementZone.bounds;
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
