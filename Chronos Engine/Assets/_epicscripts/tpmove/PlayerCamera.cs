using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public PlayerInput playerInput;

    [Header("Settings")]
    public float mouseSensitivity = 1.0f;
    public float minVerticalAngle = -45f;
    public float maxVerticalAngle = 85f;

    private Vector2 cameraInput;
    private float currentXRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; // Optionally hide the cursor
    }

    void Update()
    {
        // Read the input value every frame
        cameraInput = playerInput.actions["Look"].ReadValue<Vector2>();

        // Calculate rotation based on input and sensitivity
        float mouseX = cameraInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = cameraInput.y * mouseSensitivity * Time.deltaTime;

        // Calculate vertical rotation (pitch)
        currentXRotation -= mouseY;
        currentXRotation = Mathf.Clamp(currentXRotation, minVerticalAngle, maxVerticalAngle);

        // Apply vertical rotation to the camera transform
        transform.localRotation = Quaternion.Euler(currentXRotation, 0f, 0f);

        // Apply horizontal rotation (yaw) to the orientation transform
        orientation.Rotate(Vector3.up * mouseX);
    }
}