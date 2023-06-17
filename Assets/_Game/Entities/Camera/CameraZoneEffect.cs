using System;
using UnityEngine;

public class CameraZoneEffect : MonoBehaviour
{
    public CameraZone cameraZone;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        cameraZone.SetActive();
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        cameraZone.SetInactive();
    }
}
