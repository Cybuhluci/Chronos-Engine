using Luci;
using UnityEngine;

public class PlayerCameraBob : MonoBehaviour
{
    public FirstPersonController playerController; // assign in inspector
    public Transform theBobber; // camera or parent transform that will be offset

    [Header("General")]
    [Tooltip("Speed at which bob amplitude lerps when starting/stopping")]
    public float smoothSpeed = 8f;
    [Tooltip("Minimum speed (m/s) to start bobbing")]
    public float minSpeedThreshold = 0.1f;

    [Header("Frequency")]
    public float walkFrequency = 6f;
    public float sprintFrequency = 9f;

    [Header("Amplitude")]
    public float walkAmplitude = 0.1f;
    public float sprintAmplitude = 0.25f;
    public float crouchMultiplier = 0.5f;
    public float downedMultiplier = 0.15f;

    [Header("Movement Rotation Bob")]
    // for walking or sprinting, how much rotation bob to apply based on player wasd movement.
    public float rotAmplitudeX = 0.6f; // pitch
    public float rotAmplitudeY = 0.6f; // yaw

    // internal
    private Vector3 _originalLocalPos;
    // rotation is intentionally not modified by this bobber (Cinemachine controls rotation)
    private float _timer = 0f;
    private Vector3 _currentOffset = Vector3.zero;
    private Vector3 _targetOffset = Vector3.zero;
    private Vector3 _lastAppliedOffset = Vector3.zero;


    private void Reset()
    {
        // try to auto-assign the player controller and bob transform
        if (playerController == null)
            playerController = FindAnyObjectByType<FirstPersonController>();

        // If using Cinemachine, prefer the camera target on the player controller
        if (playerController != null && playerController.CinemachineCameraTarget != null)
        {
            theBobber = playerController.CinemachineCameraTarget.transform;
        }
        else
        {
            // fallback to main camera transform
            if (theBobber == null && Camera.main != null)
                theBobber = Camera.main.transform;
        }
    }

    private void Start()
    {
        Reset();
        if (theBobber != null)
        {
            _originalLocalPos = theBobber.localPosition;
        }
    }

    private void Update()
    {
        if (playerController == null || theBobber == null) return;

        var cc = playerController.GetComponent<CharacterController>();
        Vector3 horizontalVel = Vector3.zero;
        if (cc != null)
            horizontalVel = new Vector3(cc.velocity.x, 0f, cc.velocity.z);

        float speed = horizontalVel.magnitude;

        // determine multipliers based on player state
        float stateAmpMult = 1f;
        float stateFreqMult = 1f;
        switch (playerController._playerState)
        {
            case FirstPersonController.PlayerState.Crouching:
                stateAmpMult = crouchMultiplier;
                stateFreqMult = 0.85f;
                break;
            case FirstPersonController.PlayerState.Downed:
                stateAmpMult = downedMultiplier;
                stateFreqMult = 0.5f;
                break;
            default:
                stateAmpMult = 1f;
                stateFreqMult = 1f;
                break;
        }

        // decide frequency and amplitude by whether sprinting or not
        bool sprinting = playerController.isSprinting;

        float freq = sprinting ? sprintFrequency : walkFrequency;
        float amp = sprinting ? sprintAmplitude : walkAmplitude;

        freq *= stateFreqMult;
        amp *= stateAmpMult;

        // If player is in air (jumping/falling), stop bobbing and decay timer toward zero
        bool isAirborne = !playerController.Grounded || Mathf.Abs(playerController.playerVelocity.y) > 0.1f;
        if (isAirborne)
        {
            _timer = Mathf.Lerp(_timer, 0f, Time.deltaTime * smoothSpeed * 0.5f);
            _targetOffset = Vector3.zero;
        }
        // when standing still, slow down timer and lerp offsets back to zero
        else if (speed < minSpeedThreshold)
        {
            // gently return to rest
            _timer = Mathf.Lerp(_timer, 0f, Time.deltaTime * smoothSpeed * 0.5f);
            _targetOffset = Vector3.zero;
        }
        else
        {
            // advance bob timer based on speed and frequency (speed normalized by sprint speed)
            float speedFactor = Mathf.Clamp01(speed / Mathf.Max(0.0001f, playerController.SprintSpeed));
            _timer += Time.deltaTime * freq * (0.5f + 0.5f * speedFactor);

            // compute target offsets
            float horiz = Mathf.Sin(_timer) * amp; // sway left-right
            float vert = Mathf.Sin(_timer * 2f + Mathf.PI / 2f) * (amp * 0.6f); // up-down faster

            // slight forward/back bob to simulate stride
            float fwd = Mathf.Cos(_timer) * (amp * 0.15f);

            _targetOffset = new Vector3(horiz, vert, fwd);

            // rotation targets intentionally not applied to avoid overwriting controller pitch.
            // (rotAmplitude fields remain for future use if desired)
        }

        // smooth current towards target (position only)
        _currentOffset = Vector3.Lerp(_currentOffset, _targetOffset, Time.deltaTime * smoothSpeed);
    }

    private void LateUpdate()
    {
        if (theBobber == null) return;

        // apply positional bob additively without modifying rotation
        Vector3 basePos = theBobber.localPosition;
        theBobber.localPosition = basePos - _lastAppliedOffset + _currentOffset;
        _lastAppliedOffset = _currentOffset;
    }
}
