using Luci;
using UnityEngine;

public class WeaponInertia : MonoBehaviour
{
    public enum InertiaStyle
    {
        None, // no inertia at all, weapon will be perfectly still in the player's view.
        Basic, // a simple inertia that lags behind the camera based on mouse movement. It's pretty basic, but it gets the job done and is very performant.
        Advanced // a more advanced inertia that takes into account player movement as well as mouse movement, for a more immersive feel.
    }

    public FirstPersonController playerContoller;

    [Header("Inertia Settings")]
    public InertiaStyle inertiaStyle = InertiaStyle.Basic;
    public Transform GunHandler;
    [Tooltip("Optional: if set, use this transform's rotation as the camera reference (eg Cinemachine target). Otherwise Camera.main is used.")]
    public Transform PlayerCamRoot;

    [Header("Basic Parameters")]
    public float rotationMultiplier = 1.0f;
    public float positionMultiplier = 0.004f;
    [Tooltip("Separate multiplier for vertical (pitch) positional offset to emphasize up/down inertia")]
    public float verticalPositionMultiplier = 0.01f;
    public float smoothSpeed = 8f;
    public float maxAngularOffset = 8f;
    public float maxPosOffset = 0.06f;

    // runtime state
    private Vector3 _originalLocalPos;
    private Quaternion _originalLocalRot;
    private Vector3 _currentLocalPos;
    private Quaternion _currentLocalRot;
    private float _lastCamYaw;
    private float _lastCamPitch;
    private bool _initialized = false;

    void Start()
    {
        if (GunHandler == null) GunHandler = transform;
        _originalLocalPos = GunHandler.localPosition;
        _originalLocalRot = GunHandler.localRotation;
        _currentLocalPos = _originalLocalPos;
        _currentLocalRot = _originalLocalRot;

        Transform camT = PlayerCamRoot != null ? PlayerCamRoot : (Camera.main != null ? Camera.main.transform : null);
        if (camT != null)
        {
            Vector3 e = camT.eulerAngles;
            _lastCamYaw = e.y;
            _lastCamPitch = e.x;
            _initialized = true;
        }
    }

    void Update()
    {
        // if sprinting, reset any applied inertia immediately and skip applying inertia
        if (playerContoller != null && playerContoller.isSprinting)
        {
            ResetInertia();
            return;
        }
        if (inertiaStyle == InertiaStyle.None) return;

        if (inertiaStyle == InertiaStyle.Basic)
            BasicInertia();
        else if (inertiaStyle == InertiaStyle.Advanced)
            AdvancedInertia();
    }

    private void BasicInertia()
    {
        if (!_initialized || GunHandler == null) return;

        Transform camT = PlayerCamRoot != null ? PlayerCamRoot : (Camera.main != null ? Camera.main.transform : null);
        if (camT == null) return;

        Vector3 e = camT.eulerAngles;
        float yaw = e.y;
        float pitch = e.x;

        float dy = Mathf.DeltaAngle(_lastCamYaw, yaw);
        float dp = Mathf.DeltaAngle(_lastCamPitch, pitch);

        _lastCamYaw = yaw;
        _lastCamPitch = pitch;

        // rotation target: small rotation opposite to camera delta to simulate inertia
        Vector3 rotTargetEuler = new Vector3(-dp * rotationMultiplier, -dy * rotationMultiplier, 0f);
        rotTargetEuler = Vector3.ClampMagnitude(rotTargetEuler, maxAngularOffset);
        Quaternion rotTarget = Quaternion.Euler(rotTargetEuler);

        // position target: slight offset based on camera motion
        // allow a stronger vertical response (pitch) via verticalPositionMultiplier
        Vector3 posTarget = new Vector3(-dy * positionMultiplier, -dp * verticalPositionMultiplier, 0f);
        posTarget = Vector3.ClampMagnitude(posTarget, maxPosOffset);

        // lerp current towards target
        _currentLocalRot = Quaternion.Slerp(_currentLocalRot, _originalLocalRot * rotTarget, Time.deltaTime * smoothSpeed);
        _currentLocalPos = Vector3.Lerp(_currentLocalPos, _originalLocalPos + posTarget, Time.deltaTime * smoothSpeed);

        GunHandler.localRotation = _currentLocalRot;
        GunHandler.localPosition = _currentLocalPos;
    }

    // placeholder for advanced inertia; can include movement-based sway and inertia
    private void AdvancedInertia()
    {
        // for now, advanced uses basic behavior
        BasicInertia();
    }

    private void OnDisable()
    {
        if (GunHandler != null)
        {
            GunHandler.localPosition = _originalLocalPos;
            GunHandler.localRotation = _originalLocalRot;
        }
    }

    private void ResetInertia()
    {
        if (GunHandler == null) return;
        _currentLocalPos = _originalLocalPos;
        _currentLocalRot = _originalLocalRot;
        GunHandler.localPosition = _originalLocalPos;
        GunHandler.localRotation = _originalLocalRot;
    }
}
