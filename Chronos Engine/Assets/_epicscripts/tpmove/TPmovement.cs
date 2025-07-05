using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class TPmovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform lookCamera;
    public PlayerInput playerInput;

    [Header("Movement")]
    public Vector2 direction;
    public camerastyle cameraStyle;
    public movementstate movementState;
    public float moveSpeed;
    public float currentspeed;
    public float speedWalk = 1.5f;
    public float speedRun = 3f;
    public float speedFastRun = 6f;
    public float speedBoost = 12f;

    public InputActionAsset playermovement;
    private Vector2 lookinput;

    public Animator anim;

    public float playerRotationSpeed = 0.05f;
    public float turnSmoothVelocity;

    public CinemachineFreeLook freeLookCamera;
    public CinemachineVirtualCamera virtualCamera;
    public CinemachineBrain brain;

    public GameObject playergameobject;
    public Transform playertransform;
    public Transform FPcamtransform;

    public float sensX, sensY;
    public float xRotation, yRotation;

    private float targetAngle;

    public GameObject pushobject;
    public float pushSpeed;
    public LayerMask PushLayers;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundLayer;
    public float gravity = -9.81f;
    private Vector3 velocity;
    public bool isGrounded;

    private Vector3 lastPosition;

    public bool canMove = true;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        movementState = movementstate.idle;
        cameraStyle = camerastyle.third;
        moveSpeed = speedWalk;

        lastPosition = transform.position;
    }

    public enum camerastyle
    {
        third,
        first,
        second,
        pushingobject
    }

    public enum movementstate
    {
        idle,
        walking,
        running,
        sprinting
    }

    // Update is called once per frame
    void Update()
    {
        // Toggle between walking and running
        if (playerInput.actions["walk"].WasPressedThisFrame())
        {
            if (movementState == movementstate.walking)
            {
                movementState = movementstate.running;
                moveSpeed = speedFastRun;
            }
            else
            {
                movementState = movementstate.walking;
                moveSpeed = speedWalk;
            }
        }

        direction = playerInput.actions["regmove"].ReadValue<Vector2>();

        CameraAndMovement();
        Perspective();
        animations();

        velocity.y = -10f; // small downward push to keep grounded
        controller.Move(velocity * Time.deltaTime);

        // **Ground Check**
        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundDistance, groundLayer);
    }

    void CameraAndMovement()
    {
        if (PlayerPrefs.GetInt("CameraDisable", 0) == 1) return;

        // camera rotation
        if (cameraStyle == camerastyle.third)
        {
            if (direction.magnitude >= 0.1f)
            {
                targetAngle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg + lookCamera.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, playerRotationSpeed);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
                Vector3 movedirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                controller.Move(movedirection.normalized * moveSpeed * Time.deltaTime);
            }
        }
        else if (cameraStyle == camerastyle.first)
        {
            lookinput = playerInput.actions["Look"].ReadValue<Vector2>();

            float mouseX = lookinput.x * Time.deltaTime * sensX;
            float mouseY = lookinput.y * Time.deltaTime * sensY;

            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -85f, 85f);

            FPcamtransform.rotation = Quaternion.Euler(xRotation, yRotation, 0);

            if (direction.magnitude >= 0.1f)
            {
                // Use the camera's forward direction to calculate movement
                Vector3 moveDirection = FPcamtransform.forward * direction.y + FPcamtransform.right * direction.x;
                moveDirection.y = 0f; // Ignore vertical movement for grounded movement

                // Move the player
                controller.Move(moveDirection.normalized * moveSpeed * Time.deltaTime);
            }
        }
        else if (cameraStyle == camerastyle.pushingobject)
        {
            // Get forward input only
            float forwardInput = direction.y;

            if (pushobject != null)
            {
                // Lock player facing the object
                Vector3 directionToObject = pushobject.transform.position - transform.position;
                directionToObject.y = 0; // Keep it horizontal
                Quaternion targetRotation = Quaternion.LookRotation(directionToObject);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

                // Player's movement while pushing object (Move player smoothly with animation)
                Vector3 moveDirection = transform.forward * forwardInput;
                controller.Move(moveDirection * moveSpeed * Time.deltaTime);

                // Move the object with Rigidbody
                Rigidbody rb = pushobject.GetComponent<Rigidbody>();
                if (rb != null && forwardInput > 0.1f) // Only push forward, not pull
                {
                    // Calculate the push force (we can tweak this as needed)
                    Vector3 pushDirection = transform.forward * pushSpeed * Time.deltaTime;

                    // Make sure we're applying the force only on the desired object layers
                    if ((PushLayers.value & (1 << pushobject.layer)) != 0)
                    {
                        // Apply force to the object, similar to BasicRigidBodyPush behavior
                        rb.AddForce(pushDirection, ForceMode.Impulse);

                        // Optionally add a BoxCast to avoid clipping through walls (same logic as before)
                        Collider objectCollider = pushobject.GetComponent<Collider>();
                        Vector3 halfExtents = objectCollider.bounds.extents;
                        float distance = Vector3.Distance(rb.position, rb.position + pushDirection);

                        if (!Physics.BoxCast(rb.position, halfExtents, transform.forward, out RaycastHit hitInfo, rb.rotation, distance, ~0, QueryTriggerInteraction.Ignore))
                        {
                            // If path is clear, move the object
                            rb.MovePosition(rb.position + pushDirection);
                        }
                    }
                }
            }
        }
    }

    void Perspective()
    {
        // camera type changes
        if (playerInput.actions["firstperson"].WasPressedThisFrame()) // Switch to first-person
        {
            ChangeToFirst();
        }
        else if (playerInput.actions["secondperson"].WasPressedThisFrame()) // Switch to second-person
        {
            ChangeToSecond();
        }
        else if (playerInput.actions["thirdperson"].WasPressedThisFrame()) // Switch to third-person
        {
            ChangeToThird();
        }
    }

    public void ChangeToFirst() // camera in player head
    {
        playergameobject.SetActive(false);
        virtualCamera.Priority = 11;

        //xRotation = 0;

        // Align FPcam's rotation with the orientation's horizontal direction
        yRotation = playertransform.eulerAngles.y;
        FPcamtransform.rotation = Quaternion.Euler(xRotation, yRotation, 0);

        cameraStyle = camerastyle.first;
    }

    public void ChangeToSecond() // camera from view of security camera or something
    { 
        // nothing to be here yet, since im still not sure about second person
    } 

    public void ChangeToThird() // camera floating behind player
    {
        playergameobject.SetActive(true);
        virtualCamera.Priority = 9;

        cameraStyle = camerastyle.third;
    }

    public float horizontalSpeed;

    void animations()
    {
        // Calculate movement speed manually (distance per second)
        float distanceMoved = (transform.position - lastPosition).magnitude;
        float calculatedSpeed = distanceMoved / Time.deltaTime;

        // Update animator
        anim.SetFloat("Speed", calculatedSpeed, 0.1f, Time.deltaTime);

        pushSpeed = calculatedSpeed;

        // Store current position for next frame
        lastPosition = transform.position;
    }

    private void OnDrawGizmos()
    {
        // Debug line to visualize playerObj forward direction
        Debug.DrawLine(playertransform.position, playertransform.position + playertransform.forward * 2, Color.yellow);

        // Debug line to visualize the camera's direction from above the player's position
        Vector3 cameraDirection = lookCamera.transform.forward; // camera's forward direction
        Vector3 elevatedPosition = playertransform.position + Vector3.up * 1.8f; // Elevate the line to the player's head height
        Debug.DrawLine(elevatedPosition, elevatedPosition + cameraDirection * 2, Color.cyan);

        // Debug line to visualize playerObj forward direction
        Debug.DrawLine(FPcamtransform.position, FPcamtransform.position + FPcamtransform.forward * 2, Color.red);

        // Visualize FreeLook camera's forward direction (yellow)
        Debug.DrawLine(lookCamera.position, lookCamera.position + lookCamera.forward * 2, Color.yellow);

        // Visualize First-Person camera's forward direction (blue)
        Debug.DrawLine(FPcamtransform.position, FPcamtransform.position + FPcamtransform.forward * 2, Color.blue);
    }
}