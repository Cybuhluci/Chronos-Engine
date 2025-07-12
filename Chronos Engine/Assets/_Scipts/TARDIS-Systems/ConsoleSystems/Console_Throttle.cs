using UnityEngine;
using Luci.TARDIS;
using Luci.TARDIS.Engine;

namespace Luci.TARDIS.Console
{
    public class Console_Throttle : TARDISSubsystemController
    {
        [Header("Dependencies")]
        [SerializeField] private TARDISMain tardisMain;
        [SerializeField] private TARDISConsoleManager consoleManager;
        [SerializeField] private TARDISEngineManager engineManager;

        [Header("Throttle Settings")]
        [Range(0, 11)] public int currentThrottleValue = 0; // 0 = off, 11 = full throttle
        public float throttleChangeSpeed = 0.1f; // How fast throttle increases/decreases flight speed

        // Public property to get the current throttle value
        public float CurrentThrottleValue => currentThrottleValue;

        // We'll set the initial state here. A throttle is typically "off" (not actively thrusting) at start.
        void Awake()
        {
            // Ensure the throttle starts at zero.
            // _isCircuitActive from the base class defaults to false, which is correct for an 'inactive' throttle.
            SetThrottle(0); // Ensure value is 0
            Debug.Log($"Console_Throttle: Initial state - {GetCircuitStatus()}, Value: {currentThrottleValue}");
        }

        // --- TARDISSubsystemController Implementations ---

        // This method is called by the base ToggleCircuit() when _isCircuitActive becomes TRUE.
        // For the throttle, 'Activated' means it's ready to accept input and adjust speed.
        protected override void OnCircuitActivated()
        {
            Debug.Log("Console_Throttle: CONTROL ENGAGED (Ready to adjust).");
        }

        // This method is called by the base ToggleCircuit() when _isCircuitActive becomes FALSE.
        // For the throttle, 'Deactivated' means it cannot be adjusted, and typically resets to zero.
        protected override void OnCircuitDeactivated()
        {
            SetThrottle(0); // Reset throttle to 0 when deactivated
            Debug.Log("Console_Throttle: CONTROL DISENGAGED (Throttle reset to 0).");
        }

        // This method provides the current status string.
        public override string GetCircuitStatus()
        {
            // Indicate if the control is active, and its current value
            return _isCircuitActive ? $"Control Engaged (Value: {currentThrottleValue})" : $"Control Disengaged (Value: {currentThrottleValue})";
        }

        // --- Throttle Specific Methods ---

        // Increases the throttle value by 1
        public void IncreaseThrottle()
        {
            if (!isFunctional)
            {
                Debug.Log("Throttle: Cannot increase, not functional.");
                return;
            }

            // If throttle is off and circuit is not active, activate it first
            if (!_isCircuitActive && currentThrottleValue == 0)
            {
                ToggleCircuit();
            }

            // Only adjust if control is active and functional
            if (!_isCircuitActive)
            {
                Debug.Log("Throttle: Cannot increase, control is disengaged.");
                return;
            }

            currentThrottleValue = Mathf.Min(currentThrottleValue + 1, 11);
            Debug.Log($"Console_Throttle: INCREASED to {currentThrottleValue}");
        }

        // Decreases the throttle value by 1
        public void DecreaseThrottle()
        {
            if (!_isCircuitActive || !isFunctional)
            {
                Debug.Log("Throttle: Cannot decrease, control is disengaged or not functional.");
                return;
            }

            currentThrottleValue = Mathf.Max(currentThrottleValue - 1, 0);
            Debug.Log($"Console_Throttle: DECREASED to {currentThrottleValue}");

            // If throttle reaches 0, deactivate the circuit
            if (currentThrottleValue == 0)
            {
                ToggleCircuit();
            }
        }

        // Sets the throttle to a specific value (e.g., for quick presets)
        public void SetThrottle(int value) // Changed parameter to int for consistency with currentThrottleValue
        {
            if (!_isCircuitActive && value != 0) // Cannot set non-zero if control is disengaged
            {
                Debug.Log("Throttle: Cannot set non-zero value, control is disengaged.");
                return;
            }
            if (!isFunctional) // Cannot set if not functional
            {
                Debug.Log("Throttle: Cannot set, not functional.");
                return;
            }

            currentThrottleValue = Mathf.Clamp(value, 0, 11);
            Debug.Log($"Console_Throttle: SET to {currentThrottleValue}");
        }
    }
}