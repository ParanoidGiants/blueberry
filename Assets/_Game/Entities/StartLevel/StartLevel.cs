using Cinemachine;
using Creeper;
using Unity.VisualScripting;
using UnityEngine;

public class StartLevel : MonoBehaviour
{
    private InputController _inputController;
    public CinemachineVirtualCamera camera;

    private void Awake()
    {
        _inputController = FindObjectOfType<InputController>();
        _inputController.FreezeInputs();
        FindObjectOfType<HeadController>().transform.position = transform.position;
        FindObjectOfType<CameraController>().SetCameraActive(false);
    }

    public void OnStartLevel()
    {
        _inputController.UnfreezeInputs();
        FindObjectOfType<CameraController>().SetCameraActive(true);
    }
}
