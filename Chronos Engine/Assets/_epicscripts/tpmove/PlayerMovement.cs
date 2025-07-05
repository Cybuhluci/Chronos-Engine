using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] PlayerAttributes playerAttributes;
    [SerializeField] PlayerCamera Camera;

    [Header("Keybinds")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Animations")]
    [SerializeField] private Animator animator;

    [Header("Movement")]
    [SerializeField] Vector2 moveDirection;
    [SerializeField] private CharacterController Controller;
    
    [Space(5)]
    [SerializeField] private float CurrentMoveSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;
    private float targetSpeed;

    [Space(5)]
    [SerializeField] private float WalkSpeed;
    bool IsRunning;
    [SerializeField] private float RunSpeed;
    bool IsSprinting;
    [SerializeField] private float SprintSpeed;

    [Space(5)]
    [SerializeField] private Transform orientation;
    [SerializeField] private float groundDrag;

    [Header("Jumping")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;
    bool readyToJump;

    [Header("Camera")]
    [SerializeField] private Transform CameraBrain;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask GroundLayer;
    bool IsGrounded;
}
