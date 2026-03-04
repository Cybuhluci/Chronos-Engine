using UnityEngine;
using UnityEngine.InputSystem;

namespace Luci
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] MissionManager _MissionManager;
        public enum PlayerState { Standing, Crouching, Downed }
        public PlayerState _playerState = PlayerState.Standing;
        public bool CameraDisable = false;
        public bool MovementDisable = false;

        [Header("Player")]
        public bool noclipEnabled = false;
        public float MoveSpeed = 4.0f;
        public float SprintSpeed = 6.0f;
        public float RotationSpeed = 1.0f;
        public float SpeedChangeRate = 10.0f;

        [Space(10)]
        public float JumpHeight = 1.2f;
        public float Gravity = -15.0f;

        [Space(10)]
        public float JumpTimeout = 0.1f;
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        public bool Grounded = true;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.5f;
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        public GameObject CinemachineCameraTarget;
        public float TopClamp = 90.0f;
        public float BottomClamp = -90.0f;

        [Header("Stamina System")]
        public float Stamina = 100f;
        public float MaxStamina = 100f;
        public float StaminaDrainRate = 20f;
        public float StaminaRegenRate = 15f;
        public float MinSprintStamina = 10f;

        [Header("Crouch Settings")]
        public float CrouchSpeedMultiplier = 0.5f;
        public float StandingHeight = 2.0f;
        public float CrouchHeight = 1.0f;
        public float CrouchCameraYOffset = -0.5f;

        [Header("Downed Settings")]
        public float DownedHeight = 0.5f;
        public float DownedCameraYOffset = -1.0f;

        [Header("Camera Settings")]
        public float MouseSensitivity = 1.0f;
        [Tooltip("Separate sensitivity for gamepads/joysticks (already normalized) ")]
        public float GamepadSensitivity = 120f;
        [Tooltip("Smoothing time in seconds for camera rotation (higher = smoother/slower) ")]
        public float rotationSmoothTime = 0.03f;

        private float smoothYawVelocity;
        private float smoothPitchVelocity;
        private float targetYaw;
        private float targetPitch;

        [Header("Collision")]
        public float CapsuleCastSkin = 0.05f; // small gap to prevent immediate collision

        // internal state
        private float _lastCrouchPressTime = -10f;
        private float _crouchDoublePressThreshold = 0.3f;

        private Vector3 _originalCameraLocalPos;
        private float _originalControllerHeight;
        private Vector3 _originalControllerCenter;

        // camera & movement
        private float _cinemachineTargetPitch;
        private float _speed;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timers
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        private PlayerInput _playerInput;
        private CharacterController _controller;
        private GameObject _mainCamera;
        public ConsoleManager consoleManager;

        private const float _threshold = 0.01f;

        // Dolphin dive internals removed

        private float _heightLerpSpeed = 10f;
        private float _targetControllerHeight;
        private Vector3 _targetControllerCenter;
        private Vector3 _cameraLocalPosTarget;
        private Vector3 _originalControllerCenterCached;

        // For dynamic ground check adjustment
        private float _originalGroundedRadius;
        private float _originalGroundedOffset;

        private void Awake()
        {
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _playerInput = GetComponent<PlayerInput>();
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            _originalCameraLocalPos = CinemachineCameraTarget != null ? CinemachineCameraTarget.transform.localPosition : Vector3.zero;
            _originalControllerHeight = _controller.height;
            _originalControllerCenter = _controller.center;
            _originalControllerCenterCached = _originalControllerCenter;

            _targetControllerHeight = _controller.height;
            _targetControllerCenter = _controller.center;
            _cameraLocalPosTarget = _originalCameraLocalPos;

            // Store original ground check values
            _originalGroundedRadius = GroundedRadius;
            _originalGroundedOffset = GroundedOffset;

            Cursor.lockState = CursorLockMode.Locked;

            // initialize rotation targets to current orientation to avoid snap
            targetYaw = transform.localEulerAngles.y;
            if (CinemachineCameraTarget != null)
            {
                float pitch = CinemachineCameraTarget.transform.localEulerAngles.x;
                if (pitch > 180f) pitch -= 360f;
                targetPitch = pitch;
            }

            _MissionManager = MissionManager.Instance;
        }

        private void Update()
        {
            // Keep original ground check values
            GroundedRadius = _originalGroundedRadius;
            GroundedOffset = _originalGroundedOffset;
            HandleTimers();
            HandleInputsAndStates();
            HandleStanceSmooth();
            HandleStamina();
            JumpAndGravity();
            GroundedCheck();
            // sync noclip state if CharacterController was toggled externally
            if (_controller != null)
            {
                if (!_controller.enabled && !noclipEnabled)
                    ToggleNoclip(true);
                else if (_controller.enabled && noclipEnabled)
                    ToggleNoclip(false);
            }

            Move();
            DeveloperConsoleBind();
        }

        void DeveloperConsoleBind()
        {
            var consoleEnabled = consoleManager.consoleActive;
            if (_playerInput != null && _playerInput.actions["Console"] != null && _playerInput.actions["Console"].WasPressedThisFrame())
            {
                consoleManager.ToggleConsole();
                if (consoleManager.consoleActive)
                {
                    Cursor.lockState = CursorLockMode.None;
                    ToggleDisableCamera(true);
                    ToggleDisableMovement(true);
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    ToggleDisableCamera(false);
                    ToggleDisableMovement(false);
                }
            }
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void HandleTimers()
        {
            if (_jumpTimeoutDelta > 0f) _jumpTimeoutDelta -= Time.deltaTime;
            if (_fallTimeoutDelta > 0f) _fallTimeoutDelta -= Time.deltaTime;
        }

        private void HandleInputsAndStates()
        {
            if (MovementDisable) return;
            bool crouchPressed = false;
            bool sprintHeld = false;
            if (_playerInput != null)
            {
                var a = _playerInput.actions;
                if (a["Crouch"] != null) crouchPressed = a["Crouch"].WasPressedThisFrame();
                if (a["Sprint"] != null) sprintHeld = a["Sprint"].IsPressed();
            }
            else
            {
                crouchPressed = Input.GetKeyDown(KeyCode.C);
                sprintHeld = Input.GetKey(KeyCode.LeftShift);
            }
            // stance handling
            if (crouchPressed)
            {
                if (_playerState == PlayerState.Crouching)
                {
                    SetPlayerState(PlayerState.Standing);
                }
                else
                {
                    SetPlayerState(PlayerState.Crouching);
                }
            }
        }

        private void HandleStanceSmooth()
        {
            switch (_playerState)
            {
                case PlayerState.Standing:
                    _targetControllerHeight = StandingHeight;
                    _cameraLocalPosTarget = _originalCameraLocalPos;
                    _targetControllerCenter = _originalControllerCenterCached;
                    break;
                case PlayerState.Crouching:
                    _targetControllerHeight = CrouchHeight;
                    _cameraLocalPosTarget = _originalCameraLocalPos + new Vector3(0, CrouchCameraYOffset, 0);
                    _targetControllerCenter = new Vector3(_originalControllerCenterCached.x, CrouchHeight / 2f, _originalControllerCenterCached.z);
                    break;
                case PlayerState.Downed:
                    _targetControllerHeight = DownedHeight;
                    _cameraLocalPosTarget = _originalCameraLocalPos + new Vector3(0, DownedCameraYOffset, 0);
                    _targetControllerCenter = new Vector3(_originalControllerCenterCached.x, DownedHeight / 2f, _originalControllerCenterCached.z);
                    break;
            }

            _controller.height = Mathf.Lerp(_controller.height, _targetControllerHeight, Time.deltaTime * _heightLerpSpeed);
            _controller.center = Vector3.Lerp(_controller.center, _targetControllerCenter, Time.deltaTime * _heightLerpSpeed);

            if (CinemachineCameraTarget != null)
            {
                CinemachineCameraTarget.transform.localPosition = Vector3.Lerp(CinemachineCameraTarget.transform.localPosition, _cameraLocalPosTarget, Time.deltaTime * _heightLerpSpeed);
            }
        }

        private void HandleStamina()
        {
            bool sprint = false;
            Vector2 move = Vector2.zero;
            if (_playerInput != null)
            {
                var a = _playerInput.actions;
                if (a["Sprint"] != null) sprint = a["Sprint"].IsPressed();
                if (a["Move"] != null) move = a["Move"].ReadValue<Vector2>();
            }
            else
            {
                sprint = Input.GetKey(KeyCode.LeftShift);
                move = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            }
            if (sprint && move != Vector2.zero && Stamina > 0f)
            {
                Stamina -= StaminaDrainRate * Time.deltaTime;
                if (Stamina < 0f) Stamina = 0f;
            }
            else
            {
                Stamina += StaminaRegenRate * Time.deltaTime;
                if (Stamina > MaxStamina) Stamina = MaxStamina;
            }
        }

        private void CameraRotation()
        {
            if (CameraDisable) return;
            Vector2 look = Vector2.zero;
            if (_playerInput != null)
            {
                var action = _playerInput.actions["Look"];
                if (action != null) look = action.ReadValue<Vector2>();
            }
            else
            {
                look = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            }

            bool isMouse = IsCurrentDeviceMouse();

            float deltaX, deltaY;
            if (isMouse)
            {
                // Look action provides mouse delta which is already frame-dependent in the input system (when using "delta")
                // Scale directly by sensitivity
                deltaX = look.x * MouseSensitivity;
                deltaY = look.y * MouseSensitivity;
            }
            else
            {
                // Gamepad / joystick axes should be scaled per second for frame-rate independence
                deltaX = look.x * GamepadSensitivity * Time.deltaTime;
                deltaY = look.y * GamepadSensitivity * Time.deltaTime;
            }

            // accumulate target rotations (apply rotation speed multiplier)
            targetYaw += deltaX * RotationSpeed;
            targetPitch -= deltaY * RotationSpeed;

            // clamp pitch
            targetPitch = ClampAngle(targetPitch, BottomClamp, TopClamp);

            // smooth
            float smoothYaw = Mathf.SmoothDampAngle(transform.localEulerAngles.y, targetYaw, ref smoothYawVelocity, rotationSmoothTime);
            float smoothPitch = Mathf.SmoothDampAngle(CinemachineCameraTarget != null ? CinemachineCameraTarget.transform.localEulerAngles.x : 0f, targetPitch, ref smoothPitchVelocity, rotationSmoothTime);

            // apply
            transform.localRotation = Quaternion.Euler(0f, smoothYaw, 0f);
            if (CinemachineCameraTarget != null)
                CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(smoothPitch, 0f, 0f);
        }

        private void Move()
        {
            if (MovementDisable) return;

            if (noclipEnabled)
            {
                MoveNoclip();
                return;
            }
            bool sprint = false;
            Vector2 move = Vector2.zero;
            if (_playerInput != null)
            {
                var a = _playerInput.actions;
                if (a["Sprint"] != null) sprint = a["Sprint"].IsPressed();
                if (a["Move"] != null) move = a["Move"].ReadValue<Vector2>();
            }
            else
            {
                sprint = Input.GetKey(KeyCode.LeftShift);
                move = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            }

            // Normal movement
            float stateSpeedMultiplier = 1.0f;
            switch (_playerState)
            {
                case PlayerState.Crouching:
                    stateSpeedMultiplier = CrouchSpeedMultiplier;
                    break;
                case PlayerState.Downed:
                    stateSpeedMultiplier = 0.15f;
                    break;
            }

            float targetSpeed = (sprint && Stamina > MinSprintStamina) ? SprintSpeed : MoveSpeed;
            targetSpeed *= stateSpeedMultiplier;
            if (move == Vector2.zero) targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
            float speedOffset = 0.1f;
            float inputMagnitude = (move != Vector2.zero) ? move.magnitude : 1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            Vector3 inputDirection = Vector3.zero;
            if (move != Vector2.zero)
            {
                inputDirection = transform.right * move.x + transform.forward * move.y;
                inputDirection = inputDirection.normalized;
            }

            Vector3 desiredMove = inputDirection * _speed;
            Vector3 displacementNoY = desiredMove * Time.deltaTime;
            Vector3 totalDisplacement = displacementNoY + Vector3.up * _verticalVelocity * Time.deltaTime;

            Vector3 safe = ComputeSafeDisplacement(totalDisplacement);
            _controller.Move(safe);
        }

        // Noclip movement: ignores collisions and gravity, moves in camera look direction so "look up and walk" works
        private void MoveNoclip()
        {
            // determine input
            Vector2 move = Vector2.zero;
            bool sprint = false;
            bool jumpHeld = false;
            bool descendHeld = false;
            if (_playerInput != null)
            {
                var a = _playerInput.actions;
                if (a["Move"] != null) move = a["Move"].ReadValue<Vector2>();
                if (a["Sprint"] != null) sprint = a["Sprint"].IsPressed();
                if (a["Jump"] != null) jumpHeld = a["Jump"].IsPressed();
                if (a["Crouch"] != null) descendHeld = a["Crouch"].IsPressed();
            }
            else
            {
                move = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
                sprint = Input.GetKey(KeyCode.LeftShift);
                jumpHeld = Input.GetButton("Jump");
                descendHeld = Input.GetKey(KeyCode.C);
            }

            // pick a reference forward (camera target if present, else main camera, else transform)
            Transform refT = CinemachineCameraTarget != null ? CinemachineCameraTarget.transform : (Camera.main != null ? Camera.main.transform : transform);

            // build direction: forward uses full camera forward (includes vertical) so looking up and walking moves up
            Vector3 forward = refT.forward;
            Vector3 right = refT.right;

            Vector3 dir = (right * move.x) + (forward * move.y);

            // vertical ascend/descend
            if (jumpHeld) dir += Vector3.up;
            if (descendHeld) dir += Vector3.down;

            if (dir.sqrMagnitude > 0.0001f) dir = dir.normalized;

            float speed = sprint ? SprintSpeed : MoveSpeed;
            Vector3 delta = dir * speed * Time.deltaTime;

            // move transform directly (ignores collisions)
            transform.position += delta;
        }

        private Vector3 ComputeSafeDisplacement(Vector3 desiredDisplacement)
        {
            float radius = Mathf.Max(0.01f, _controller.radius);
            float halfHeight = Mathf.Max(radius, (_controller.height * 0.5f) - radius);
            Vector3 center = transform.position + _controller.center;

            Vector3 p1 = center + Vector3.up * halfHeight;
            Vector3 p2 = center - Vector3.up * halfHeight;

            Vector3 horizDisplacement = new Vector3(desiredDisplacement.x, 0f, desiredDisplacement.z);
            float distance = horizDisplacement.magnitude;

            if (distance > 0.001f)
            {
                Vector3 dir = horizDisplacement.normalized;
                if (Physics.CapsuleCast(p1, p2, radius, dir, out RaycastHit hit, distance + CapsuleCastSkin, ~0, QueryTriggerInteraction.Ignore))
                {
                    float moveDist = Mathf.Max(0f, hit.distance - CapsuleCastSkin);
                    Vector3 movePart = dir * moveDist;
                    Vector3 remaining = horizDisplacement - movePart;
                    Vector3 slide = Vector3.ProjectOnPlane(remaining, hit.normal);
                    Vector3 result = movePart + slide + Vector3.up * desiredDisplacement.y;
                    return result;
                }
            }

            return desiredDisplacement;
        }

        private void JumpAndGravity()
        {
            // when noclip is active, do not apply gravity or jumping
            if (noclipEnabled) return;

            // apply gravity
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }

            if (_MissionManager.GetPlayerState() == MissionManager.PlayerState.Casing) return;

            if (MovementDisable || _playerState == PlayerState.Downed) return;
            bool jump = false;
            if (_playerInput != null)
            {
                var a = _playerInput.actions;
                if (a["Jump"] != null) jump = a["Jump"].WasPressedThisFrame();
            }
            else
            {
                jump = Input.GetButtonDown("Jump");
            }

            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                if (jump && _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                }
            }
            else
            {
                _jumpTimeoutDelta = JumpTimeout;
            }
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
        }

        private PlayerState preDownPlayerState;

        public void SetDownedState(bool isDowned)
        {
            if (isDowned)
            {
                preDownPlayerState = _playerState; // store current state to return to later
                _playerState = PlayerState.Downed;
                SetPlayerState(PlayerState.Downed);
            }
            else
            {
                if (_playerState == PlayerState.Downed)
                {
                    _playerState = preDownPlayerState;
                    SetPlayerState(preDownPlayerState);
                }
            }
        }

        private void SetPlayerState(PlayerState newState)
        {
            if (_playerState == newState) return;
            _playerState = newState;

            //// If downed, disable movement but allow camera rotation. Also set target stance to prone height.
            //if (_playerState == PlayerState.Downed)
            //{
            //    // set target controller height to downed values so player visually goes down
            //    _targetControllerHeight = DownedHeight;
            //    _cameraLocalPosTarget = _originalCameraLocalPos + new Vector3(0, DownedCameraYOffset, 0);
            //    _targetControllerCenter = new Vector3(_originalControllerCenterCached.x, DownedHeight / 2f, _originalControllerCenterCached.z);
            //}
            //else
            //{
            //    // leaving downed state re-enable movement
            //    ToggleDisableMovement(false);
            //}
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private bool IsCurrentDeviceMouse()
        {
            if (_playerInput == null) return true;
            return _playerInput.currentControlScheme == "KeyboardMouse";
        }

        public void ToggleDisableCamera(bool set)
        {
            CameraDisable = set;
        }

        public void ToggleDisableMovement(bool set)
        {
            MovementDisable = set;
        }

        // Enable or disable noclip mode. When enabled CharacterController is disabled and movement ignores collisions/gravity.
        public void ToggleNoclip(bool enable)
        {
            noclipEnabled = enable;
            if (_controller != null)
            {
                _controller.enabled = !enable;
            }
            // reset vertical velocity so gravity does not immediately apply when toggling
            _verticalVelocity = 0f;
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // Draw the ground check sphere (wireframe for clarity)
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Gizmos.DrawWireSphere(spherePosition, GroundedRadius);
        }
    }
}