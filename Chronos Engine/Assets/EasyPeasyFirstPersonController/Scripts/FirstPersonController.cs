using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class FirstPersonController : MonoBehaviour
{
    [Header("Dependencies")] // Grouping essential references
    public Transform playerCamera;
    public Transform cameraParent;
    public Transform groundCheck;
    public CharacterController characterController; // Made public for direct assignment in Inspector if needed
    public Camera cam; // Made public for direct assignment if needed
    public CinemachineBrain brain; // Cinemachine Brain for camera control
    public AudioSource slideAudioSource; // Made public for direct assignment if needed
    [SerializeField] PlayerInput playerinput; // Already serialized, good!

    [Header("Mouse Look Settings")]
    [Range(0, 100)] public float mouseSensitivity = 25f;
    [Range(0f, 200f)] private float snappiness = 100f; // For smooth camera interpolation
    private float rotX, rotY; // Current rotation values
    private float xVelocity, yVelocity; // For smooth damping
    private Vector2 cameraInputPlayerInput; // Input from PlayerInput for look

    [Header("Movement Speeds")]
    [Range(0f, 20f)] public float walkSpeed = 10f;
    [Range(0f, 30f)] public float sprintSpeed = 15f;
    [Range(0f, 10f)] public float crouchSpeed = 6f;
    public float slideSpeed = 9f;

    [Header("Movement Abilities")]
    public bool canSlide = true;
    public bool canJump = true;
    public bool canSprint = true;
    public bool canCrouch = true;

    [Header("Crouch Settings")]
    public float crouchHeight = 1f;
    public float crouchCameraHeight = 0.5f;
    private float originalHeight; // Stores original CharacterController height
    private float originalCameraParentHeight; // Stores original camera parent height
    private float currentCameraHeight; // Current interpolated camera height

    [Header("Slide Settings")]
    public float slideDuration = 0.7f;
    public float slideFovBoost = 5f;
    public float slideTiltAngle = 5f;
    private float slideTimer;
    private float postSlideCrouchTimer;
    private Vector3 slideDirection; // Direction of the slide
    private float currentSlideSpeed; // Current interpolated slide speed
    private float slideSpeedVelocity; // For smooth damping
    private float currentTiltAngle; // Current interpolated camera tilt
    private float tiltVelocity; // For smooth damping

    [Header("Jump & Gravity")]
    [Range(0f, 15f)] public float jumpSpeed = 3f;
    [Range(0f, 50f)] public float gravity = 9.81f;
    public bool coyoteTimeEnabled = true;
    public float coyoteTimeDuration = 0.25f;
    private float coyoteTimer;
    private Vector3 moveDirection = Vector3.zero; // Current movement vector including gravity

    [Header("Ground Check")]
    public float groundDistance = 0.3f;
    public LayerMask groundMask;
    private bool isGrounded;

    [Header("FOV & Headbob")]
    public float normalFov = 60f;
    public float sprintFov = 70f;
    public float fovChangeSpeed = 5f;
    public float walkingBobbingSpeed = 14f;
    public float bobbingAmount = 0.05f;
    private float sprintBobMultiplier = 1.2f;
    private float recoilReturnSpeed = 8f;
    private float bobTimer;
    private float defaultPosY; // Original Y position for headbob calculations
    private Vector3 recoil = Vector3.zero; // For camera recoil effects
    private float currentBobOffset; // Current interpolated headbob offset
    private float currentFov; // Current interpolated FOV
    private float fovVelocity; // For smooth damping

    [Header("Current State")]
    public bool isSprinting;
    public bool isCrouching;
    public bool isSliding;
    private bool isLook = true; // Control flag for camera look
    private bool isMove = true; // Control flag for player movement

    // Input System specific variables
    private Vector2 moveInputPlayerInput; // Input from PlayerInput for movement (WASD)

    // Public property for external access to camera height
    public float CurrentCameraHeight => isCrouching || isSliding ? crouchCameraHeight : originalCameraParentHeight;

    private void Awake()
    {
        // Get component references if not assigned in Inspector
        if (characterController == null) characterController = GetComponent<CharacterController>();
        if (cam == null) cam = playerCamera.GetComponent<Camera>();
        if (playerinput == null) playerinput = GetComponent<PlayerInput>(); // Ensure PlayerInput is referenced

        originalHeight = characterController.height;
        originalCameraParentHeight = cameraParent.localPosition.y;
        defaultPosY = cameraParent.localPosition.y;

        // Add AudioSource if not already present, or get existing one
        if (slideAudioSource == null)
        {
            slideAudioSource = gameObject.AddComponent<AudioSource>();
            slideAudioSource.playOnAwake = false;
            slideAudioSource.loop = false;
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; // Usually want cursor invisible when locked

        currentCameraHeight = originalCameraParentHeight;
        currentBobOffset = 0f;
        currentFov = normalFov;
        currentSlideSpeed = 0f;
        currentTiltAngle = 0f;

        // Ensure PlayerInput is enabled on Awake
        if (playerinput != null)
        {
            playerinput.enabled = true;
        }
    }

    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && moveDirection.y < 0)
        {
            moveDirection.y = -2f;
            coyoteTimer = coyoteTimeEnabled ? coyoteTimeDuration : 0f;
        }
        else if (coyoteTimeEnabled)
        {
            coyoteTimer -= Time.deltaTime;
        }

        HandleLook();
        HandleHeadBob();
        HandleCrouchSlideState(); // Renamed and consolidated crouch/slide logic
        HandleMovement();
    }

    // Cinemachine First Person Look: Rotates cameraParent (pitch) and player (yaw)
    // Ensure your CinemachineVirtualCamera's Follow and LookAt targets are set to cameraParent.
    private void HandleLook()
    {
        if (isLook)
        {
            cameraInputPlayerInput = playerinput.actions["Look"].ReadValue<Vector2>();

            float mouseX = cameraInputPlayerInput.x * mouseSensitivity * Time.deltaTime;
            float mouseY = cameraInputPlayerInput.y * mouseSensitivity * Time.deltaTime;

            rotX += mouseX;
            rotY -= mouseY;
            rotY = Mathf.Clamp(rotY, -90f, 90f);

            xVelocity = Mathf.Lerp(xVelocity, rotX, snappiness * Time.deltaTime);
            yVelocity = Mathf.Lerp(yVelocity, rotY, snappiness * Time.deltaTime);

            float targetTiltAngle = isSliding ? slideTiltAngle : 0f;
            currentTiltAngle = Mathf.SmoothDamp(currentTiltAngle, targetTiltAngle, ref tiltVelocity, 0.2f);

            // Pitch (vertical look) on cameraParent, with tilt for sliding
            cameraParent.localRotation = Quaternion.Euler(yVelocity - currentTiltAngle, 0f, 0f);
            // Yaw (horizontal look) on player body
            transform.rotation = Quaternion.Euler(0f, xVelocity, 0f);
        }
    }

    private void HandleCrouchSlideState()
    {
        // Check if player *wants* to crouch based on input
        bool wantsToCrouch = canCrouch && playerinput.actions["Crouch"].IsPressed() && !isSliding;

        // CapsuleCast for ceiling check (prevent standing up into ceiling)
        Vector3 point1 = transform.position + characterController.center - Vector3.up * (characterController.height * 0.5f);
        Vector3 point2 = point1 + Vector3.up * characterController.height * 0.6f;
        float capsuleRadius = characterController.radius * 0.95f;
        float castDistance = isSliding ? originalHeight + 0.2f : originalHeight - crouchHeight + 0.2f;
        bool hasCeiling = Physics.CapsuleCast(point1, point2, capsuleRadius, Vector3.up, castDistance, groundMask);

        // Handle post-slide crouch timer
        if (isSliding)
        {
            postSlideCrouchTimer = 0.3f;
        }
        if (postSlideCrouchTimer > 0)
        {
            postSlideCrouchTimer -= Time.deltaTime;
            isCrouching = canCrouch; // Force crouch during this small window after slide
        }
        else
        {
            // Determine if truly crouching based on input or ceiling
            isCrouching = canCrouch && (wantsToCrouch || (hasCeiling && !isSliding));
        }

        // Handle Slide initiation (now linked to the same Crouch input button press)
        // We trigger slide only once on the *down* press of the crouch button, while sprinting and grounded.
        // Also check if jump button was NOT pressed to prevent slide on jump.
        if (canSlide && isSprinting && playerinput.actions["Crouch"].WasPressedThisFrame() && isGrounded)
        {
            // Prevent multiple slide initiations if crouch is held
            if (!isSliding) 
            {
                isSliding = true;
                slideTimer = slideDuration;
                // Calculate slide direction based on current move input, or forward if standing still
                slideDirection = moveInputPlayerInput.magnitude > 0.1f ? (transform.right * moveInputPlayerInput.x + transform.forward * moveInputPlayerInput.y).normalized : transform.forward;
                currentSlideSpeed = sprintSpeed; // Start slide with sprint speed
                // Play slide sound if you have one
                // if (slideAudioSource != null && slideAudioSource.clip != null) slideAudioSource.Play();
            }
        }

        // Handle active sliding
        float slideProgress = slideTimer / slideDuration;
        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0f || !isGrounded) // End slide if timer runs out or airborne
            {
                isSliding = false;
            }
            float targetSlideSpeed = slideSpeed * Mathf.Lerp(0.7f, 1f, slideProgress);
            currentSlideSpeed = Mathf.SmoothDamp(currentSlideSpeed, targetSlideSpeed, ref slideSpeedVelocity, 0.2f);
            characterController.Move(slideDirection * currentSlideSpeed * Time.deltaTime);
        }

        // Adjust CharacterController height and center for crouch/slide
        float targetHeight = isCrouching || isSliding ? crouchHeight : originalHeight;
        characterController.height = Mathf.Lerp(characterController.height, targetHeight, Time.deltaTime * 10f);
        characterController.center = new Vector3(0f, characterController.height * 0.5f, 0f);

        // Adjust FOV based on sprint/slide state
        float targetFov = isSprinting ? sprintFov : (isSliding ? sprintFov + (slideFovBoost * Mathf.Lerp(0f, 1f, 1f - slideProgress)) : normalFov);
        currentFov = Mathf.SmoothDamp(currentFov, targetFov, ref fovVelocity, 1f / fovChangeSpeed);
        cam.fieldOfView = currentFov;
    }

    private void HandleHeadBob()
    {
        Vector3 horizontalVelocity = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z);
        bool isMovingEnough = horizontalVelocity.magnitude > 0.1f;

        // Calculate targetCameraHeight ONCE, as it's needed regardless of grounded state
        float targetCameraHeight = isCrouching || isSliding ? crouchCameraHeight : originalCameraParentHeight;

        // Headbob offset calculation
        float targetBobOffset = isMovingEnough && isGrounded && !isSliding && !isCrouching ? Mathf.Sin(bobTimer) * bobbingAmount : 0f;
        currentBobOffset = Mathf.Lerp(currentBobOffset, targetBobOffset, Time.deltaTime * walkingBobbingSpeed);

        // Apply camera height and bob offset
        currentCameraHeight = Mathf.Lerp(currentCameraHeight, targetCameraHeight, Time.deltaTime * 10f);
        cameraParent.localPosition = new Vector3(
            cameraParent.localPosition.x,
            currentCameraHeight + currentBobOffset,
            cameraParent.localPosition.z);

        // Handle recoil logic
        if (!isGrounded || isSliding || isCrouching || !isMovingEnough) // Clear recoil if conditions aren't met for movement recoil
        {
            recoil = Vector3.zero;
            bobTimer = 0f; // Reset bob timer if not moving or specific conditions met
        }
        else // Only apply recoil and increment bobTimer if grounded and moving
        {
            float bobSpeed = walkingBobbingSpeed * (isSprinting ? sprintBobMultiplier : 1f);
            bobTimer += Time.deltaTime * bobSpeed;
            recoil.z = moveInputPlayerInput.x * -2f; // Apply horizontal recoil
        }

        cameraParent.localRotation = Quaternion.RotateTowards(cameraParent.localRotation, Quaternion.Euler(recoil), recoilReturnSpeed * Time.deltaTime);
    }

    private void HandleMovement()
    {
        // Read move input directly from PlayerInput actions
        moveInputPlayerInput = playerinput.actions["Move"].ReadValue<Vector2>();

        // Update sprinting status based on PlayerInput
        isSprinting = canSprint 
                      && playerinput.actions["Sprint"].IsPressed() 
                      && moveInputPlayerInput.y > 0.1f 
                      && isGrounded 
                      && !isCrouching 
                      && !isSliding;

        float currentSpeed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);
        if (!isMove) currentSpeed = 0f;

        Vector3 direction = new Vector3(moveInputPlayerInput.x, 0f, moveInputPlayerInput.y);
        Vector3 moveVector = transform.TransformDirection(direction) * currentSpeed;
        moveVector = Vector3.ClampMagnitude(moveVector, currentSpeed);

        // Jump logic
        if (playerinput.actions["Jump"].WasPressedThisFrame() && (isGrounded || coyoteTimer > 0f) && canJump && !isSliding)
        {
            moveDirection.y = jumpSpeed;
        }
        else if (isGrounded && moveDirection.y < 0)
        {
            moveDirection.y = -2f; // Keep player grounded
        }
        else
        {
            moveDirection.y -= gravity * Time.deltaTime; // Apply gravity
        }

        if (!isSliding)
        {
            moveDirection = new Vector3(moveVector.x, moveDirection.y, moveVector.z);
            characterController.Move(moveDirection * Time.deltaTime);
        }
    }

    // --- Public Control Methods ---
    // These methods control the player's ability to move and look, useful for UI, cutscenes, etc.

    public void SetControl(bool newState)
    {
        SetLookControl(newState);
        SetMoveControl(newState);
    }

    public void SetLookControl(bool newState)
    {
        isLook = newState;
        // If look is disabled, clear any lingering input to prevent unwanted camera movement
        if (!newState)
        {
            cameraInputPlayerInput = Vector2.zero;
        }
    }

    public void SetMoveControl(bool newState)
    {
        isMove = newState;
        // If movement is disabled, clear any lingering input and reset movement states
        if (!newState)
        {
            moveInputPlayerInput = Vector2.zero;
            isSprinting = false;
            isCrouching = false; // Ensure crouch state is reset
            isSliding = false; // Ensure slide state is reset
        }
    }

    public void SetCursorVisibility(bool newVisibility)
    {
        Cursor.lockState = newVisibility ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = newVisibility;
    }
}