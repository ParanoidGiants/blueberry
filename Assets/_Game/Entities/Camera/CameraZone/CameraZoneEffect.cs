using System;
using UnityEngine;

namespace GameCamera
{
    public class CameraZoneEffect : MonoBehaviour
    {
        private CameraZone _cameraZone;

        private void Awake()
        {
            _cameraZone = GetComponentInParent<CameraZone>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            
            _cameraZone.SetActive();
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            
            _cameraZone.SetInactive();
        }
    }
}
