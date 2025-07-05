using Unity.Cinemachine;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TPBOOST2movement : MonoBehaviour
{
    public CharacterController controller;
    public Transform lookCamera;

    [Header("Movement")]
    public camerastyle cameraStyle;
    public movementstate movementState;
    public float currentSpeed;
    public float maxSpeed;
    public float acceleration = 8f; // Speed up rate
    public float deceleration = 6f; // Slow down rate
    public float speedWalk = 3f;
    public float speedRun = 6f;
    public float speedFastRun = 12f;
    public float speedBoost = 24f;
    public int stateofmovement;

    public InputActionAsset playermovement;
    private InputAction playerregmove;
    private InputAction playerlook;
    private InputAction playerrun;
    private InputAction playerjump;
    private InputAction playerboost;
    private InputAction playerslide;

    public Animator anim;
    public TMP_Text speedText;

    public float playerRotationSpeed = 0.05f;
    public float turnSmoothVelocity;

    public FTPpausescript pausescript;

    public CinemachineFreeLook freeLookCamera;
    public CinemachineBrain brain;

    public GameObject playergameobject;
    public Transform playertransform;
    public GameObject TPcam;

    private float targetAngle;

    public float jumpHeight = 2f;
    public float jumpHoldTime = 0.2f;
    public float gravity = -9.81f;
    public float jumpTimer;
    public Vector3 velocity;
    public bool isGrounded;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundLayer;
    private bool jumped;

    public float currentBoostEnergy = 100f;
    public float boostEnergyDrainRate = 2f;
    public bool isBoosting = false;
    public Slider boostGauge;
    private movementstate laststatebeforeboost;

    // Sliding Variables
    public bool isSliding = false;
    public float slideDuration = 1.2f; // How long the slide lasts
    public float slideSpeedMultiplier = 1.5f; // How much speed increases during slide
    private float normalHeight;
    public float slideHeight = 1f; // Smaller hitbox during slide
    private Vector3 slideDownForce = new Vector3(0, -0.5f, 0); // Small downward push
    private Vector3 slideEndUpForce = new Vector3(0, 1f, 0); // Small upward push
    public float slideheightstart;
    public float slideheightend;

    private Vector3 previousPosition;
    private Vector3 gravityDirection = Vector3.down;

    // Start is called before the first frame update
    void Start()
    {
        playerlook = playermovement.FindActionMap("Player").FindAction("Look");
        playerregmove = playermovement.FindActionMap("Player").FindAction("regmove");
        playerjump = playermovement.FindActionMap("Player").FindAction("jump");
        playerboost = playermovement.FindActionMap("Player").FindAction("boost");
        playerslide = playermovement.FindActionMap("Player").FindAction("crouchslide");

        playerboost.Enable();
        playerjump.Enable();
        playerslide.Enable();

        normalHeight = controller.height;
        movementState = movementstate.idle;
        previousPosition = transform.position;

    }

    public enum camerastyle
    {
        third, first
    }

    public enum movementstate
    {
        idle,
        walking,
        running,
        fastrunning,
        boosting,
        inair,
        sliding,
        stomping,
        homing,
        lightdashing
    }

    // Update is called once per frame
    void Update()
    {
        if (pausescript.inPauseScreen == false)
        {
            brain.enabled = true;
            playerlook.Enable();
            Vector2 direction = playerregmove.ReadValue<Vector2>();

            // **Ground Check**
            isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundDistance, groundLayer);
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
                jumped = false;
            }

            // Boosting now instantly reaches max speed while normal movement has acceleration
            if (playerboost.IsPressed() && currentBoostEnergy > 0)
            {
                laststatebeforeboost = movementState;
                isBoosting = true;
                currentSpeed = speedBoost; // Instantly reach boost speed
                currentBoostEnergy -= boostEnergyDrainRate * Time.deltaTime;
                movementState = movementstate.boosting;
                anim.SetBool("isBoosting", true);
                if (currentBoostEnergy <= 0) currentBoostEnergy = 0;
            }
            else if (!playerboost.IsPressed() || currentBoostEnergy < 0)
            {
                isBoosting = false;
                movementState = laststatebeforeboost;
                anim.SetBool("isBoosting", false);
                maxSpeed = (direction.magnitude > 0.1f) ? speedFastRun : 0; // Normal movement still uses acceleration
            }

            // **Sliding Mechanic**
            if (playerslide.IsPressed() && isGrounded && direction.magnitude > 0.1f && !isSliding)
            {
                StartSlide();
            }
            else if (!playerslide.IsPressed() && isSliding) // Stop sliding when the key is released
            {
                StopSlide();
            }

            // **Apply Momentum for Normal Movement**
            if (!isBoosting && !isSliding)
            {
                if (direction.magnitude > 0.1f)
                {
                    currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
                }
                else
                {
                    currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.deltaTime);
                }
            }

            boostGauge.value = currentBoostEnergy;

            // Jumping Mechanic
            if (playerjump.WasPressedThisFrame() && isGrounded)
            {
                jumpTimer = 0;
                Jump();
            }

            if (playerjump.IsPressed() && jumpTimer < jumpHoldTime)
            {
                velocity.y += Mathf.Sqrt(jumpHeight * -2f * gravity) * Time.deltaTime;
                jumpTimer += Time.deltaTime;
            }

            // **Ground Check & Surface Alignment**
            RaycastHit hit;
            if (Physics.Raycast(groundCheck.position, -playertransform.up, out hit, groundDistance * 2, groundLayer))
            {
                isGrounded = true;
                jumped = false; // Reset jump state
                Vector3 surfaceNormal = hit.normal;

                // **Align Player to the Surface Normal**
                Quaternion targetRotation = Quaternion.FromToRotation(playertransform.up, surfaceNormal) * playertransform.rotation;
                playertransform.rotation = Quaternion.Slerp(playertransform.rotation, targetRotation, Time.deltaTime * 10f);

                // **Set Gravity Direction**
                gravityDirection = surfaceNormal;
            }
            else
            {
                isGrounded = false;
            }

            // **Apply Gravity in the Local Direction**
            velocity += gravityDirection * gravity * Time.deltaTime;

            // **Fix Jumping (Now Works Again!)**
            if (playerjump.WasPressedThisFrame() && isGrounded)
            {
                velocity += playertransform.up * Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumped = true;
                isGrounded = false; // Temporarily disable ground detection to prevent instant cancelling
            }

            // Prevent velocity from building up infinitely
            if (isGrounded)
            {
                velocity.x = Mathf.Lerp(velocity.x, 0, Time.deltaTime * 5f);
                velocity.z = Mathf.Lerp(velocity.z, 0, Time.deltaTime * 5f);
            }

            // **Apply Movement**
            controller.Move(velocity * Time.deltaTime);

            // **Rotation & Movement**
            if (direction.magnitude >= 0.1f)
            {
                targetAngle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg + lookCamera.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, playerRotationSpeed);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
                Vector3 movedirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                controller.Move(movedirection.normalized * currentSpeed * Time.deltaTime);
            }

            // SPD count
            Vector3 displacement = (transform.position - previousPosition) / Time.deltaTime; // Calculate actual movement
            previousPosition = transform.position; // Update previous position for next frame

            float visualSpeed = Mathf.RoundToInt(displacement.magnitude); // Get total movement speed

            speedText.text = (visualSpeed) + " SPD";

            // **Animations**
            if (isGrounded)
            {
                anim.SetBool("isjump", false); // jumping animation
                anim.SetBool("isfalling", false); // falling animation

                if (visualSpeed <= 0.1f)
                {
                    movementState = movementstate.idle;
                    anim.SetInteger("MovementState", 0); // idle animation
                }
                else if (visualSpeed <= speedWalk)
                {
                    movementState = movementstate.walking;
                    anim.SetInteger("MovementState", 1); // walking animation
                }
                else if (visualSpeed <= speedRun)
                {
                    movementState = movementstate.running;
                    anim.SetInteger("MovementState", 2); // running animation
                }
                else if (visualSpeed <= speedFastRun)
                {
                    movementState = movementstate.fastrunning;
                    anim.SetInteger("MovementState", 3); // fast run / boost animation
                }
                else if (visualSpeed <= speedBoost)
                {
                    movementState = movementstate.fastrunning;
                    anim.SetInteger("MovementState", 3); // fast run / boost animation
                }
            }
            else
            {
                movementState = movementstate.inair;
                anim.SetBool(jumped ? "isjump" : "isfalling", true); // jumping / falling animation
            }
        }
        else
        {
            playerlook.Disable();
            brain.enabled = false;
        }
    }

    // **Slide Function**
    void StartSlide()
    {
        isSliding = true;
        controller.height = slideHeight; // Shrink player hitbox
        currentSpeed *= slideSpeedMultiplier; // Increase speed for slide

        // Push player slightly downward to stay grounded
        controller.Move(slideDownForce * Time.deltaTime);

        // Set player Y position to 0.5 to prevent sinking into the ground
        Vector3 newPosition = playertransform.position;
        newPosition.y = slideheightstart;
        playertransform.position = newPosition;

        movementState = movementstate.sliding;
        anim.SetBool("isSliding", true);
    }

    // **End Slide After Duration**
    void StopSlide()
    {
        isSliding = false;
        anim.SetBool("isSliding", false);
        controller.Move(slideEndUpForce * Time.deltaTime);
        controller.height = normalHeight; // Restore normal hitbox

        // Smoothly return Y position to 0
        Vector3 newPosition = playertransform.position;
        newPosition.y = slideheightend;
        playertransform.position = newPosition;
    }

    private void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        jumped = true;
    }

    private void OnDrawGizmos()
    {
        // Debug line to visualize playerObj forward direction
        Debug.DrawLine(playertransform.position, playertransform.position + playertransform.forward * 2, Color.yellow);

        // Debug line to visualize the camera's direction from above the player's position
        Vector3 cameraDirection = lookCamera.transform.forward; // camera's forward direction
        Vector3 elevatedPosition = playertransform.position + Vector3.up * 1.8f; // Elevate the line to the player's head height
        Debug.DrawLine(elevatedPosition, elevatedPosition + cameraDirection * 2, Color.cyan);

        // Visualize FreeLook camera's forward direction (yellow)
        Debug.DrawLine(lookCamera.position, lookCamera.position + lookCamera.forward * 2, Color.yellow);

        // Visualize The ground checker raycast (red)
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundDistance);
    }
}