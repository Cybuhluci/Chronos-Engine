using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class _42ConsoleNavigator : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    [Header("Cameras")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineCamera frontCamera;
    [SerializeField] private CinemachineCamera scannerCamera;
    [SerializeField] private CinemachineCamera telepathicCamera;
    [SerializeField] private CinemachineCamera throttleCamera;
    [SerializeField] private CinemachineCamera communicatorCamera;
    [SerializeField] private CinemachineCamera helmicCamera;
    [SerializeField] private CinemachineCamera monitor1Camera, monitor2Camera, astronavCamera;
    [SerializeField] private CinemachineCamera doorFacingCamera;

    private CinemachineCamera[] consoleCameras;
    private CinemachineCamera[] monitorCameras;

    [SerializeField] private CinemachineCamera currentCamera;

    Vector2 inputVector;
    private bool inputReady = true;
    private const float triggerThreshold = 0.6f;
    private const float resetThreshold = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        consoleCameras = new CinemachineCamera[] { frontCamera, scannerCamera, telepathicCamera, throttleCamera, communicatorCamera, helmicCamera };
        monitorCameras = new CinemachineCamera[] { monitor1Camera, monitor2Camera, astronavCamera };

        ActivateCamera(frontCamera); // Start with front camera active
    }

    // Update is called once per frame
    void Update()
    {
        inputVector = playerInput.actions["Move"].ReadValue<Vector2>();
        // interpret input into a discrete direction
        float hx = inputVector.x;
        float hy = inputVector.y;

        // determine whether horizontal or vertical is dominant
        bool horizontal = Mathf.Abs(hx) > Mathf.Abs(hy);
        int dir = 0; // -1 = negative (left or down), 1 = positive (right or up)
        if (horizontal)
        {
            if (hx > triggerThreshold) dir = 1;
            else if (hx < -triggerThreshold) dir = -1;
        }
        else
        {
            if (hy > triggerThreshold) dir = 1;
            else if (hy < -triggerThreshold) dir = -1;
        }

        // reset input readiness when stick/keys released
        if (Mathf.Abs(hx) < resetThreshold && Mathf.Abs(hy) < resetThreshold) inputReady = true;

        if (!inputReady || dir == 0) return;

        // consume this input
        inputReady = false;

        // find active camera indices
        int activeConsole = -1;
        for (int i = 0; i < consoleCameras.Length; i++) if (consoleCameras[i] != null && consoleCameras[i].Priority == 1) activeConsole = i;
        int activeMonitor = -1;
        for (int i = 0; i < monitorCameras.Length; i++) if (monitorCameras[i] != null && monitorCameras[i].Priority == 1) activeMonitor = i;

        // Map inputs per the comment specification
        // If a console camera is active
        if (activeConsole != -1)
        {
            switch (activeConsole)
            {
                case 0: // front
                    if (horizontal && dir == -1) ActivateCamera(scannerCamera);
                    else if (horizontal && dir == 1) ActivateCamera(helmicCamera);
                    else if (!horizontal && dir == 1) ActivateCamera(monitor2Camera);
                    else if (!horizontal && dir == -1) ActivateCamera(doorFacingCamera);
                    break;
                case 1: // scanner
                    if (horizontal && dir == -1) ActivateCamera(telepathicCamera);
                    else if (horizontal && dir == 1) ActivateCamera(frontCamera);
                    else if (!horizontal && dir == 1) ActivateCamera(astronavCamera);
                    break;
                case 2: // telepathic
                    if (horizontal && dir == -1) ActivateCamera(throttleCamera);
                    else if (horizontal && dir == 1) ActivateCamera(scannerCamera);
                    else if (!horizontal && dir == 1) ActivateCamera(astronavCamera);
                    break;
                case 3: // throttle
                    if (horizontal && dir == -1) ActivateCamera(communicatorCamera);
                    else if (horizontal && dir == 1) ActivateCamera(telepathicCamera);
                    else if (!horizontal && dir == 1) ActivateCamera(monitor1Camera);
                    break;
                case 4: // communicator
                    if (horizontal && dir == -1) ActivateCamera(helmicCamera);
                    else if (horizontal && dir == 1) ActivateCamera(throttleCamera);
                    else if (!horizontal && dir == 1) ActivateCamera(monitor1Camera);
                    break;
                case 5: // helmic
                    if (horizontal && dir == -1) ActivateCamera(frontCamera);
                    else if (horizontal && dir == 1) ActivateCamera(communicatorCamera);
                    else if (!horizontal && dir == 1) ActivateCamera(monitor2Camera);
                    break;
            }
            return;
        }

        // If a monitor camera is active
        if (activeMonitor != -1)
        {
            switch (activeMonitor)
            {
                case 0: // monitor1
                    if (horizontal && dir == -1) ActivateCamera(monitor2Camera);
                    else if (horizontal && dir == 1) ActivateCamera(astronavCamera);
                    else if (!horizontal && dir == -1) ActivateCamera(communicatorCamera);
                    break;
                case 1: // monitor2
                    if (horizontal && dir == -1) ActivateCamera(astronavCamera);
                    else if (horizontal && dir == 1) ActivateCamera(monitor1Camera);
                    else if (!horizontal && dir == -1) ActivateCamera(frontCamera);
                    break;
                case 2: // astronav
                    if (horizontal && dir == -1) ActivateCamera(monitor1Camera);
                    else if (horizontal && dir == 1) ActivateCamera(monitor2Camera);
                    else if (!horizontal && dir == -1) ActivateCamera(telepathicCamera);
                    break;
            }
            return;
        }

        // If door facing camera is active
        if (doorFacingCamera != null && doorFacingCamera.Priority == 1)
        {
            if (!horizontal && dir == -1) ActivateCamera(frontCamera);
            return;
        }
    }

    private void ActivateCamera(CinemachineCamera cam)
    {
        foreach (var c in consoleCameras)
        {
            c.Priority = 0;
        }
        foreach (var c in monitorCameras)
        {
            c.Priority = 0;
        }
        doorFacingCamera.Priority = 0;
        cam.Priority = 1; // Activate the selected camera
        currentCamera = cam;
    }
}
