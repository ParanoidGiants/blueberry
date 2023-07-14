using Cinemachine;
using Creeper;
using UnityEngine;

public class StartLevel : MonoBehaviour
{
    private InputController _inputController;

    private void Awake()
    {
        _inputController = FindObjectOfType<InputController>();
        _inputController.FreezeInputs();
        FindObjectOfType<HeadController>().transform.position = transform.position;
        // FindObjectOfType<CameraController>().SetCameraActive(false);
    }

    public void OnStartLevel()
    {
        _inputController.UnfreezeInputs();
        // FindObjectOfType<CameraController>().SetCameraActive(true);
    }
}
