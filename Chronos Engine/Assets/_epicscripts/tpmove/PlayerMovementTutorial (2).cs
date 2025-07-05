using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementTutorial : MonoBehaviour
{
    public CharacterController Controller;
    public Transform orientation; // Required to determine forward direction

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f; // For rotating the player based on input

    [Header("Jump")]
    public float jumpHeight = 1f;
    public float gravity = -9.81f * 2f; // Increased gravity for more responsive jumps
    public float groundedCheckRadius = 0.2f;
    public Transform groundCheck;
    public LayerMask groundMask;
    private bool isGrounded;
    private Vector3 velocity;

    private Vector2 moveInput;
    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        // Get input from the Input System
        moveInput = playerInput.actions["Move"].ReadValue<Vector2>();

        // Handle player rotation based on input
        RotatePlayer();

        // Handle gravity and ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundedCheckRadius, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small downward force to ensure sticking to the ground
        }

        // Handle jump input
        if (playerInput.actions["Jump"].triggered && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        // Calculate movement direction based on input and orientation
        Vector3 move = orientation.forward * moveInput.y + orientation.right * moveInput.x;
        move = move.normalized * moveSpeed;
        move.y = velocity.y; // Apply vertical velocity

        // Move the player using the CharacterController
        Controller.Move(move * Time.deltaTime);
    }

    private void RotatePlayer()
    {
        if (moveInput.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg + orientation.eulerAngles.y;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}