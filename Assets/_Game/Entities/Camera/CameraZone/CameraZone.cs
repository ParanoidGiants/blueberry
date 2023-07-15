using UnityEngine;
using UnityEngine.Serialization;

namespace GameCamera
{
    public class CameraZone : MonoBehaviour
    {
        private CameraController _cameraController;
        
        [Header("References")]
        [SerializeField] private Collider _effectZone;
        [SerializeField] private Collider _movementZone;
        [SerializeField] private Transform _arrowReference;

        [Space(10)]
        [Header("Settings")]
        [SerializeField] private float zoom;
        public float Zoom => zoom;
        public Quaternion Rotation => _arrowReference.rotation;
        public Bounds Bounds { get; private set; }

        private void Awake()
        {
            _cameraController = FindObjectOfType<CameraController>();
            if (_movementZone == null)
            {
                Bounds = _effectZone.bounds;
            }
            else
            {
                Bounds = _movementZone.bounds;
            }

            _arrowReference.GetComponent<Renderer>().enabled = false;
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
}