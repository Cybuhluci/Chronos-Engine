using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class TardisController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 10f;
    public float boostMultiplier = 2f;
    public float stabilizationSpeed = 2f;

    [Header("Camera Settings")]
    public float cameraSensitivity = 100f;
    public float minCameraY = -85f;
    public float maxCameraY = 85f;
    public float cameraDistance = 10f;

    [Header("Flight Effects")]
    public float spinSpeed = 30f; // Adjustable spinning speed

    [Header("References")]
    [SerializeField] public Rigidbody rb;
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public PlayerInput playerInput;
    [SerializeField] public Transform tardisModel;
    [SerializeField] public Transform tardisMover;
    [SerializeField] public CinemachineVirtualCamera cinemachineCamera;
    [SerializeField] public GameObject staticTardis;
    [SerializeField] public GameObject fpsController;

    private Vector2 cameraRotation = Vector2.zero;
    private Vector3 moveDirection = Vector3.zero;
    public bool gravityEnabled = true;
    public bool isActive = false;

    void Start()
    {
        tardisMover = transform.Find("TARDISmover");
        tardisModel = tardisMover.Find("TARDISmodel");
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponentInChildren<AudioSource>();
        playerInput = GetComponent<PlayerInput>();
        cinemachineCamera = FindObjectOfType<CinemachineVirtualCamera>();

        if (cinemachineCamera)
        {
            Transform camTransform = cinemachineCamera.transform;
            camTransform.position = tardisModel.position;
            cinemachineCamera.LookAt = tardisMover;
        }

        rb.useGravity = gravityEnabled;
        rb.linearDamping = 1f;
        ToggleTardis(false);
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleInput();
        RotateCamera();
        
        if (playerInput.actions["exit"].WasPressedThisFrame())
        {
            AttemptExitTardis();
        }
    }

    void FixedUpdate()
    {
        if (isActive)
        {
            MoveTardis();
            StabilizeTardis();
            SpinTardis();
        }

        if (!isActive)
        {
            gravityEnabled = true;
            rb.useGravity = true;
        }
    }

    void HandleInput()
    {
        if (playerInput.actions["engine"].WasPressedThisFrame())
        {
            ToggleTardis(!isActive);
        }

        if (!isActive) return;

        Vector2 xMove = playerInput.actions["xmove"].ReadValue<Vector2>();
        float yMove = playerInput.actions["ymove"].ReadValue<float>();

        if (cinemachineCamera != null)
        {
            Transform camTransform = cinemachineCamera.transform;
            Vector3 forward = camTransform.forward;
            Vector3 right = camTransform.right;
            Vector3 up = Vector3.up;

            moveDirection = (forward * xMove.y) + (right * xMove.x) + (up * yMove);
            moveDirection.Normalize();
        }

        if (playerInput.actions["gravity"].WasPressedThisFrame())
        {
            gravityEnabled = !gravityEnabled;
            rb.useGravity = gravityEnabled;
        }
    }

    void MoveTardis()
    {
        if (rb)
        {
            float currentSpeed = speed * (playerInput.actions["throttle"].IsPressed() ? boostMultiplier : 1f);
            rb.AddForce(moveDirection * currentSpeed, ForceMode.Acceleration);
        }
    }

    void StabilizeTardis()
    {
        Quaternion targetRotation = Quaternion.Euler(0, tardisMover.eulerAngles.y, 0);
        tardisMover.rotation = Quaternion.Lerp(tardisMover.rotation, targetRotation, stabilizationSpeed * Time.fixedDeltaTime);
    }

    void RotateCamera()
    {
        Vector2 lookInput = playerInput.actions["camera"].ReadValue<Vector2>();
        cameraRotation.x += lookInput.x * cameraSensitivity * Time.deltaTime;
        cameraRotation.y -= lookInput.y * cameraSensitivity * Time.deltaTime;
        cameraRotation.y = Mathf.Clamp(cameraRotation.y, minCameraY, maxCameraY);

        Quaternion newRotation = Quaternion.Euler(cameraRotation.y, cameraRotation.x, 0);
        cinemachineCamera.transform.position = tardisModel.position - (newRotation * Vector3.forward * cameraDistance);
        cinemachineCamera.transform.LookAt(tardisMover);
    }

    public float tardisleaveattemptspeed;

    void AttemptExitTardis()
    {
        tardisleaveattemptspeed = rb.linearVelocity.magnitude;
        if (!isActive && gravityEnabled && rb.linearVelocity.magnitude <= 0.01f)
        {
            staticTardis.transform.position = transform.position;
            staticTardis.transform.rotation = tardisModel.rotation;
            staticTardis.SetActive(true);
            fpsController.SetActive(true);
            fpsController.transform.position = staticTardis.transform.position + staticTardis.transform.forward * 2f;
            gameObject.SetActive(false);
        }
    }

    void ToggleTardis(bool state)
    {
        isActive = state;
        if (audioSource)
        {
            if (state) audioSource.Play();
            else audioSource.Stop();
        }
    }

    public float speedFactor;
    public float finalSpinSpeed;

    void SpinTardis()
    {
        if (!tardisModel) return;

        float speedFactor = rb.linearVelocity.magnitude;

        RaycastHit hit;
        float heightFactor = 0f;
        if (Physics.Raycast(rb.position, Vector3.down, out hit, Mathf.Infinity))
        {
            heightFactor = Mathf.Clamp(hit.distance / 100f, 0f, 1f);
        }

        float finalSpinSpeed = spinSpeed * Mathf.Max(speedFactor / 20f, heightFactor);

        tardisModel.Rotate(Vector3.up * finalSpinSpeed * Time.fixedDeltaTime, Space.Self);
    }
}
