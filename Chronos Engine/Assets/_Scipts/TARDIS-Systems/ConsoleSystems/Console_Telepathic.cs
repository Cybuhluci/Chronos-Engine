using UnityEngine;
using UnityEngine.InputSystem;
using Luci.TARDIS;
using Luci.TARDIS.Engine;

namespace Luci.TARDIS.Console
{
    public class Console_Telepathic : TARDISSubsystemController
    {
        [Header("Dependencies")]
        [SerializeField] private TARDISMain tardisMain;
        [SerializeField] private TARDISConsoleManager consoleManager;
        [SerializeField] private TARDISEngineManager engineManager;

        [Header("Input Dependencies")]
        [SerializeField] private PlayerInput playerInput;
        private Console_Handbrake handbrake;
        private Console_Throttle throttle;
        private Console_VortexFlight vortexflight;
        private Console_Refueller refueller;
        private Console_FastReturn fastreturn; // Placeholder for Fast Return action

        // Cached Input Actions
        private InputAction _toggleHandbrakeAction;
        private InputAction _throttleAxisAction;
        private InputAction _exitTelepathicAction;

        // NEW: Actions for the Plotter keys (these willnow also trigger the adjustment)
        private InputAction _clusterPlotAction; // ','
        private InputAction _galaxyPlotAction;  // '.'
        private InputAction _planetPlotAction;  // '/'
        private InputAction _pocketPlotAction;  // 'M'

        // NEW: Actions for coordinate adjustment control
        private InputAction _coordIncrementAction;    // Cycles 1, 10, 50, 100
        private InputAction _landingTypeToggleAction; // Right Control (') - toggles direction
        private InputAction _randomiserAction;        // '-' (Minus key)

        // NEW: Vortex Flight
        private InputAction _VortexFlightAction;
        private InputAction _refuellerAction;
        private InputAction _fastreturnAction;

        [Header("Current Destination")]
        public Vector3Int targetSpatialCoordinates;
        public Vector3Int targetPocketCoordinates; // Note: Only X will be clamped 1-9 for PocketPlot

        [Header("Adjustment Settings")]
        public int[] incrementAmounts = { 1, 10, 50, 100 }; // The different increment sizes
        private int _currentIncrementIndex = 0;
        public int _selectedIncrementAmount = 1; // Starts at 1

        // NEW: Flag to store the current adjustment direction (true = positive, false = negative)
        public bool _isIncrementDirectionPositive = true;

        // We still keep this enum for status display, but direct selection no longer happens independently
        private enum SelectedCoordinate { None, ClusterPlot, GalaxyPlot, PlanetPlot, PocketPlot }
        private SelectedCoordinate _lastAdjustedCoordinate = SelectedCoordinate.None; // Track last for status display

        void Awake()
        {
            // Robust dependency finding
            if (playerInput == null) playerInput = FindAnyObjectByType<PlayerInput>();
            handbrake = consoleManager.timeRotorHandbrake; // Get the handbrake from the console manager
            throttle = consoleManager.spaceTimeThrottle; // Get the throttle from the console manager
            vortexflight = consoleManager.vortexFlight; // Get the Vortex Flight from the console manager
            refueller = consoleManager.refueller; // Get the refueller from the console manager
            fastreturn = consoleManager.fastReturn; // Get the Fast Return from the console manager

            // Get manager references from TARDISMain if not already set
            if (consoleManager == null && tardisMain != null) consoleManager = tardisMain.consoleManager;
            if (engineManager == null && tardisMain != null) engineManager = tardisMain.engineManager;

            // Initialize Input Actions and subscribe to events if playerInput and actions are available
            if (playerInput != null && playerInput.actions != null)
            {
                _exitTelepathicAction = playerInput.actions.FindAction("TARDIS/ExitTelepathic");
                _toggleHandbrakeAction = playerInput.actions.FindAction("TARDIS/Handbrake");
                _throttleAxisAction = playerInput.actions.FindAction("TARDIS/Throttle");

                // NEW: Plotter actions that also trigger adjustment
                _clusterPlotAction = playerInput.actions.FindAction("TARDIS/ClusterPlot");
                _galaxyPlotAction = playerInput.actions.FindAction("TARDIS/GalaxyPlot");
                _planetPlotAction = playerInput.actions.FindAction("TARDIS/PlanetPlot");
                _pocketPlotAction = playerInput.actions.FindAction("TARDIS/PocketPlot");

                // NEW: Control actions
                _coordIncrementAction = playerInput.actions.FindAction("TARDIS/CoordIncrement");
                _landingTypeToggleAction = playerInput.actions.FindAction("TARDIS/LandingType"); // Single key for toggling direction
                _randomiserAction = playerInput.actions.FindAction("TARDIS/Randomiser");

                // NEW: Vortex Flight
                _VortexFlightAction = playerInput.actions.FindAction("TARDIS/VortexFlight");
                _refuellerAction = playerInput.actions.FindAction("TARDIS/Refueller");
                _fastreturnAction = playerInput.actions.FindAction("TARDIS/FastReturn");

                // Subscribe to input events
                _toggleHandbrakeAction.performed += ctx => OnHandbrakeToggled();
                _throttleAxisAction.performed += OnThrottlePerformed;
                _exitTelepathicAction.performed += ctx => OnExitTelepathicPerformed();

                // Subscribe Plotter actions to their adjustment methods
                _clusterPlotAction.performed += ctx => OnPlotterPressed(SelectedCoordinate.ClusterPlot);
                _galaxyPlotAction.performed += ctx => OnPlotterPressed(SelectedCoordinate.GalaxyPlot);
                _planetPlotAction.performed += ctx => OnPlotterPressed(SelectedCoordinate.PlanetPlot);
                _pocketPlotAction.performed += ctx => OnPlotterPressed(SelectedCoordinate.PocketPlot);

                // Subscribe control actions
                _coordIncrementAction.performed += ctx => OnCoordIncrement();
                _landingTypeToggleAction.performed += ctx => OnLandingTypeTogglePerformed(); // Toggle direction
                _randomiserAction.performed += ctx => OnRandomiserPerformed();

                // subscribe to Vortex Flight action
                _VortexFlightAction.performed += ctx => OnVortexFlightPerformed();
                _refuellerAction.performed += ctx => OnRefuellerPerformed();
                _fastreturnAction.performed += ctx => OnFastReturnPerformed();
            }
            else
            {
                Debug.LogError("Console_Telepathic: PlayerInput or Input Actions not found. Telepathic controls will not function.");
            }

            // Initialize the selected increment amount
            _selectedIncrementAmount = incrementAmounts[_currentIncrementIndex];
        }

        void OnEnable()
        {
            // Enable input actions when this script is enabled
            _toggleHandbrakeAction?.Enable();
            _throttleAxisAction?.Enable();
            _exitTelepathicAction?.Enable();

            _clusterPlotAction?.Enable();
            _galaxyPlotAction?.Enable();
            _planetPlotAction?.Enable();
            _pocketPlotAction?.Enable();

            _coordIncrementAction?.Enable();
            _landingTypeToggleAction?.Enable();
            _randomiserAction?.Enable();

            _VortexFlightAction?.Enable();
            _refuellerAction?.Enable();
            _fastreturnAction?.Enable();
        }

        void OnDisable()
        {
            // Disable input actions when this script is disabled
            _toggleHandbrakeAction?.Disable();
            _throttleAxisAction?.Disable();
            _exitTelepathicAction?.Disable();

            _clusterPlotAction?.Disable();
            _galaxyPlotAction?.Disable();
            _planetPlotAction?.Disable();
            _pocketPlotAction?.Disable();

            _coordIncrementAction?.Disable();
            _landingTypeToggleAction?.Disable();
            _randomiserAction?.Disable();

            _VortexFlightAction?.Disable();
            _refuellerAction?.Disable();
            _fastreturnAction?.Disable();

            // Unsubscribe to prevent memory leaks
            if (_toggleHandbrakeAction != null) _toggleHandbrakeAction.performed -= ctx => OnHandbrakeToggled();
            if (_throttleAxisAction != null) _throttleAxisAction.performed -= OnThrottlePerformed;
            if (_exitTelepathicAction != null) _exitTelepathicAction.performed -= ctx => OnExitTelepathicPerformed();

            if (_clusterPlotAction != null) _clusterPlotAction.performed -= ctx => OnPlotterPressed(SelectedCoordinate.ClusterPlot);
            if (_galaxyPlotAction != null) _galaxyPlotAction.performed -= ctx => OnPlotterPressed(SelectedCoordinate.GalaxyPlot);
            if (_planetPlotAction != null) _planetPlotAction.performed -= ctx => OnPlotterPressed(SelectedCoordinate.PlanetPlot);
            if (_pocketPlotAction != null) _pocketPlotAction.performed -= ctx => OnPlotterPressed(SelectedCoordinate.PocketPlot);

            if (_coordIncrementAction != null) _coordIncrementAction.performed -= ctx => OnCoordIncrement();
            if (_landingTypeToggleAction != null) _landingTypeToggleAction.performed -= ctx => OnLandingTypeTogglePerformed();
            if (_randomiserAction != null) _randomiserAction.performed -= ctx => OnRandomiserPerformed();

            if (_VortexFlightAction != null) _VortexFlightAction.performed -= ctx => OnVortexFlightPerformed();
            if (_refuellerAction != null) _refuellerAction.performed -= ctx => OnRefuellerPerformed();
            if (_fastreturnAction != null) _fastreturnAction.performed -= ctx => OnFastReturnPerformed();
        }

        void Update()
        {
            if (_isCircuitActive && isFunctional)
            {
                // Only update NavCom's destination here. Coordinate adjustments are now event-driven.
                if (engineManager != null && engineManager.navigationcom != null)
                {
                    engineManager.navigationcom.SetDestination(targetSpatialCoordinates, targetPocketCoordinates);
                }
            }
        }

        // --- TARDISSubsystemController Implementations ---
        protected override void OnCircuitActivated()
        {
            Debug.Log("Console_Telepathic: ENGAGED");
            playerInput.SwitchCurrentActionMap("TARDIS"); // Switch to TARDIS action map for controls
            if (tardisMain != null && tardisMain.TelepathicGUI != null)
            {
                tardisMain.TelepathicGUI.SetActive(true);
            }
            _lastAdjustedCoordinate = SelectedCoordinate.None; // Reset selection on activate
            Debug.Log("Telepathic Controls Activated.");
            Debug.Log($"Current Increment: {_selectedIncrementAmount}");
            Debug.Log($"Current Direction: {(_isIncrementDirectionPositive ? "Positive (+)" : "Negative (-)")}");
        }

        protected override void OnCircuitDeactivated()
        {
            Debug.Log("Console_Telepathic: RELEASED");
            playerInput.SwitchCurrentActionMap("Player"); // Switch back to default Player action map
            if (tardisMain != null && tardisMain.TelepathicGUI != null)
            {
                tardisMain.TelepathicGUI.SetActive(false);
            }
            _lastAdjustedCoordinate = SelectedCoordinate.None; // Clear selection on deactivate
        }

        public override string GetCircuitStatus()
        {
            string status = _isCircuitActive ? "Engaged" : "Released";
            status += $"\nLast Adjusted: {_lastAdjustedCoordinate}";
            status += $"\nIncrement Amount: {_selectedIncrementAmount}";
            status += $"\nDirection: {(_isIncrementDirectionPositive ? "Positive (+)" : "Negative (-)")}";
            return status;
        }

        // --- Telepathic Circuit Specific Methods ---

        // Handles the general throttle input (increase/decrease)
        private void OnThrottlePerformed(InputAction.CallbackContext context)
        {
            if (!_isCircuitActive || throttle == null || !throttle.isFunctional) { return; }

            float throttleDirection = context.ReadValue<float>();

            // If throttle system is inactive and player tries to increase throttle from 0, activate it
            if (!throttle.IsCircuitActive && throttleDirection > 0 && throttle.currentThrottleValue == 0)
            {
                throttle.ToggleCircuit(); // Activates the throttle control
                Debug.Log("Throttle system automatically engaged by input!");
            }

            // Only adjust throttle if the control is active
            if (throttle.IsCircuitActive)
            {
                if (throttleDirection > 0)
                {
                    throttle.IncreaseThrottle();
                }
                else if (throttleDirection < 0)
                {
                    throttle.DecreaseThrottle();
                }
            }

            // If throttle reaches 0 and handbrake is engaged, attempt rematerialization
            if (throttle.currentThrottleValue == 0 && handbrake.IsEngaged && engineManager.stabilisers != null)
            {
                Debug.Log("Throttle hit zero with handbrake engaged. Attempting rematerialization via Stabilisers.");
                engineManager.stabilisers.AttemptRematerialization();
            }
        }

        private void OnHandbrakeToggled()
        {
            if (!_isCircuitActive || handbrake == null) { return; }

            handbrake.ToggleCircuit(); // Toggle the handbrake state

            // Attempt rematerialization immediately when handbrake is toggled
            if (engineManager.stabilisers != null)
            {
                Debug.Log("Handbrake toggled. Attempting rematerialization via Stabilisers.");
                engineManager.stabilisers.AttemptRematerialization();
            }
        }

        // Method to handle the exit telepathic action
        private void OnExitTelepathicPerformed()
        {
            if (_isCircuitActive) // Only deactivate if currently active
            {
                ToggleCircuit(); // This will call OnCircuitDeactivated()
                Debug.Log("Telepathic Circuit deactivated by exit input.");
            }
        }

        // NEW: Called when a plotter key (',', '.', '/', 'M') is pressed
        private void OnPlotterPressed(SelectedCoordinate coordType)
        {
            if (!_isCircuitActive || !isFunctional)
            {
                Debug.Log("Telepathic Circuit inactive or not functional. Cannot adjust coordinate.");
                return;
            }

            _lastAdjustedCoordinate = coordType; // Update the last adjusted coordinate for status display

            int adjustment = _selectedIncrementAmount;
            if (!_isIncrementDirectionPositive)
            {
                adjustment *= -1; // Apply negative adjustment if direction is negative
            }

            switch (coordType)
            {
                case SelectedCoordinate.ClusterPlot:
                    targetSpatialCoordinates.x += adjustment;
                    Debug.Log($"Adjusted Spatial X (ClusterPlot) to: {targetSpatialCoordinates.x}");
                    break;
                case SelectedCoordinate.GalaxyPlot:
                    targetSpatialCoordinates.y += adjustment;
                    Debug.Log($"Adjusted Spatial Y (GalaxyPlot) to: {targetSpatialCoordinates.y}");
                    break;
                case SelectedCoordinate.PlanetPlot:
                    targetSpatialCoordinates.z += adjustment;
                    Debug.Log($"Adjusted Spatial Z (PlanetPlot) to: {targetSpatialCoordinates.z}");
                    break;
                case SelectedCoordinate.PocketPlot:
                    targetPocketCoordinates.x += adjustment;
                    // Clamp PocketPlot.x to be between 1 and 9 as specified
                    targetPocketCoordinates.x = Mathf.Clamp(targetPocketCoordinates.x, 1, 9);
                    Debug.Log($"Adjusted Pocket (Time) X (PocketPlot) to: {targetPocketCoordinates.x}");
                    break;
                default:
                    Debug.LogWarning("Invalid plotter key pressed.");
                    break;
            }

            // Update NavCom immediately after adjustment for responsiveness
            if (engineManager != null && engineManager.navigationcom != null)
            {
                engineManager.navigationcom.SetDestination(targetSpatialCoordinates, targetPocketCoordinates);
            }
        }

        // NEW: Method to cycle through the increment amounts (CoordIncrement)
        private void OnCoordIncrement()
        {
            if (!_isCircuitActive || !isFunctional) { return; }

            _currentIncrementIndex = (_currentIncrementIndex + 1) % incrementAmounts.Length;
            _selectedIncrementAmount = incrementAmounts[_currentIncrementIndex];
            Debug.Log($"Increment amount cycled to: {_selectedIncrementAmount}");
        }

        // NEW: Method to toggle the increment direction (LandingType)
        private void OnLandingTypeTogglePerformed()
        {
            if (!_isCircuitActive || !isFunctional) { return; }

            _isIncrementDirectionPositive = !_isIncrementDirectionPositive; // Toggle the boolean
            Debug.Log($"Coordinate adjustment direction toggled to: {(_isIncrementDirectionPositive ? "Positive (+)" : "Negative (-)")}");
        }

        // NEW: Placeholder for Randomiser action
        private void OnRandomiserPerformed()
        {
            if (!_isCircuitActive || !isFunctional) { return; }
            Debug.Log("Randomiser key (-) pressed! (Functionality not yet implemented)");
            // Add your randomisation logic here when ready
        }

        // NEW: Vortex Flight action
        private void OnVortexFlightPerformed()
        {
            if (!_isCircuitActive || !isFunctional) { return; }
            vortexflight.ToggleCircuit();
            Debug.Log($"Vortex Flight toggled! Now: {(consoleManager.vortexFlight.IsEngaged ? "Engaged" : "Released")}");
        }

        // NEW: Placeholder for Vortex Flight action
        private void OnRefuellerPerformed()
        {
            if (!_isCircuitActive || !isFunctional) { return; }
            refueller.ToggleCircuit();
            Debug.Log($"Refueller toggled! Now: {(consoleManager.refueller.IsEngaged ? "Engaged" : "Released")}");
        }

        private void OnFastReturnPerformed()
        {
            if (!_isCircuitActive || !isFunctional) { return; }
            fastreturn.ToggleCircuit();
            Debug.Log("Fast Return action performed!");
        }

        public Vector3Int GetDestination()
        {
            return targetSpatialCoordinates;
        }

        public Vector3Int GetPocketDestination()
        {
            return targetPocketCoordinates;
        }
    }
}