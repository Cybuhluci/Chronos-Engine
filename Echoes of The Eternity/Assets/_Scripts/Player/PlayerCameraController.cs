using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Transform cameraTransform;

    void Update()
    {
        // mouse input = "Look" action in the Input System
        Vector2 lookInput = playerInput.actions["Look"].ReadValue<Vector2>();
        // Use the lookInput to rotate the cameratransform on the x and y axis, locking the y rotation to prevent flipping

    }
}
