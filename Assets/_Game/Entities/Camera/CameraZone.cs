using System;
using Creeper;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraZone : MonoBehaviour
{
    [Header("References")]
    public CameraZoneEffect effectZone;
    public Collider movementZone;
    [SerializeField] private Transform arrowReference;

    [Space(10)]
    [Header("Settings")]
    [SerializeField] private bool fixRotation;
    
    public bool FixRotation => fixRotation;
    public Vector3 position => arrowReference.position;
    public Quaternion rotation => arrowReference.rotation;

    public Bounds Bounds { get; private set; }

    private bool _isActive = false;
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
