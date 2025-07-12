using UnityEngine;
using TMPro;
using Luci.TARDIS.Engine;
using Luci.TARDIS.Console;

namespace Luci.TARDIS
{
    /// <summary>
    /// Main TARDIS control class that manages the overall state and functionality of the TARDIS.
    /// Handles flight, dematerialization, materialization, and subsystem interactions.
    /// </summary>
    /// <remarks>
    /// This class serves as the central hub for TARDIS operations, coordinating between various subsystems
    /// and managing the TARDIS's state throughout its lifecycle.
    /// </remarks>


    public class TARDISMain : MonoBehaviour
    {
        [Header("TARDIS Managers")]
        public TARDISConsoleManager consoleManager;
        public TARDISEngineManager engineManager; // Ensure this manager has a public reference to Engine_Stabilisers

        [Header("TARDIS State")]
        public TARDISState currentTARDISState = TARDISState.Landed;
        public Vector3Int currentSpatialLocation;
        public Vector3Int currentPocketLocation;

        // A flag to prevent multiple "destination reached" notifications/triggers
        private bool _destinationNotificationSent = false;

        [Header("Telepathic GUI")]
        public GameObject TelepathicGUI;
        public TMP_Text CurrentSpatial;
        public TMP_Text CurrentPocket;
        public TMP_Text DestinationSpatial;
        public TMP_Text DestinationPocket;
        public TMP_Text FlightPercent;
        public TMP_Text CoordinateIncrement;
        public TMP_Text VerticalLandingType;
        public TMP_Text FuelPercent;
        public TMP_Text ThrottleAmount; // This is the throttle amount text, not a reference to Console_Throttle
        public TMP_Text HandbrakeStatus; // This is the handbrake status text, not a reference to Console_Handbrake
        public TMP_Text VortexFlight;
        public TMP_Text Refueller;
        public TMP_Text FastReturn;
        public TMP_Text Wcoordinate;
        public TMP_Text Xcoordinate;
        public TMP_Text Ycoordinate;
        public TMP_Text Zcoordinate;

        void Awake()
        {
            if (consoleManager == null) consoleManager = GetComponentInChildren<TARDISConsoleManager>();
            if (engineManager == null) engineManager = GetComponentInChildren<TARDISEngineManager>();

            if (consoleManager == null) Debug.LogError("TARDISMain: TARDISConsoleManager reference is missing!");
            if (engineManager == null) Debug.LogError("TARDISMain: TARDISEngineManager reference is missing!");

            currentTARDISState = TARDISState.Landed;
            currentSpatialLocation = Vector3Int.zero; // Initial location
            currentPocketLocation = Vector3Int.zero; // Initial location
        }

        void Update()
        {
            HandleTARDISState(); // Manages overall TARDIS state transitions

            // --- Fast Return override: allow flight even if throttle is 0 ---
            if (currentTARDISState == TARDISState.Landed &&
                consoleManager != null &&
                consoleManager.fastReturn != null &&
                consoleManager.fastReturn.IsEngaged &&
                consoleManager.timeRotorHandbrake != null &&
                !consoleManager.timeRotorHandbrake.IsEngaged &&
                consoleManager.spaceTimeThrottle != null &&
                consoleManager.spaceTimeThrottle.currentThrottleValue == 0)
            {
                // Set throttle to 11 and engage flight
                consoleManager.spaceTimeThrottle.SetThrottle(11);
                EngageFlight();
            }

            // --- Core Flight and Landing Logic ---
            if (currentTARDISState == TARDISState.Landed)
            {
                AttemptAutoDematerialization();
                // Reset destination notification flag when landed, allowing a new flight to trigger it again
                _destinationNotificationSent = false;
                // Demat checks are now centralized in Engine_Demat.AttemptFlight
            }
            else if (currentTARDISState == TARDISState.Flying)
            {
                ControlFlight();
            }

            TelepathicGUIUpdate();
        }

        private void TelepathicGUIUpdate()
        {
            if (consoleManager == null || consoleManager.telepathicCircuit == null ||
                engineManager == null || engineManager.navigationcom == null)
            {
                Debug.LogWarning("TelepathicGUIUpdate: Missing required manager/circuit references. Cannot update GUI.");
                return;
            }

            Vector3Int currentS = engineManager.navigationcom.GetCurrentSpatial();
            Vector3Int currentP = engineManager.navigationcom.GetCurrentPocket();

            Vector3Int destS = engineManager.navigationcom.GetDestinationSpatial();
            Vector3Int destP = engineManager.navigationcom.GetDestinationPocket();

            if (CurrentSpatial != null) CurrentSpatial.text = $"Current Spatial: {currentS.x}, {currentS.y}, {currentS.z}";
            if (CurrentPocket != null) CurrentPocket.text = $"Current Pocket: {currentP.x}, {currentP.y}, {currentP.z}";
            if (DestinationSpatial != null) DestinationSpatial.text = $"Dest Spatial: {destS.x}, {destS.y}, {destS.z}";
            if (DestinationPocket != null) DestinationPocket.text = $"Dest Pocket: {destP.x}, {destP.y}, {destP.z}";

            // --- New coordinate TMP_Texts for W, X, Y, Z ---
            if (Wcoordinate != null)
                Wcoordinate.text = $"Crnt: {currentP.x}\nDest: {destP.x}";
            if (Xcoordinate != null)
                Xcoordinate.text = $"Crnt: {currentS.x}\nDest: {destS.x}";
            if (Ycoordinate != null)
                Ycoordinate.text = $"Crnt: {currentS.y}\nDest: {destS.y}";
            if (Zcoordinate != null)
                Zcoordinate.text = $"Crnt: {currentS.z}\nDest: {destS.z}";

            if (FlightPercent != null)
            {
                float progress = engineManager.navigationcom.GetFlightProgress();
                FlightPercent.text = $"Flight Progress: {(progress * 100):F1}%";
            }

            if (consoleManager.deltaCircuit.isIncrementDirectionPositive == false)
            {
                VerticalLandingType.text = $"Vertical Landing Type: Down";
            }
            else
            {
                VerticalLandingType.text = $"Vertical Landing Type: Up";
            }
            CoordinateIncrement.text = $"Increment: {consoleManager.deltaCircuit._selectedIncrementAmount}";

            FuelPercent.text = $"Fuel Left: {engineManager.fluidlinks.FuelLeft:F1}%"; // Assuming FuelLeft is a float percentage

            ThrottleAmount.text = $"Throttle Amount: {consoleManager.spaceTimeThrottle.currentThrottleValue}";
            HandbrakeStatus.text = $"Handbrake Status: {(consoleManager.timeRotorHandbrake.IsEngaged ? "Active" : "Inactive")}";

            VortexFlight.text = $"Vortex Flight: {(consoleManager.vortexFlight != null && consoleManager.vortexFlight.IsEngaged ? "Active" : "Inactive")}";
            Refueller.text = $"Refueller: {(consoleManager.refueller != null && consoleManager.refueller.IsEngaged ? "Refuelling" : "Idle")}";
            FastReturn.text = $"Fast Return: {(consoleManager.fastReturn != null && consoleManager.fastReturn.IsEngaged ? "Active" : "Inactive")}";
        }

        private void AttemptAutoDematerialization()
        {
            if (currentTARDISState == TARDISState.Landed)
            {
                if (engineManager != null && engineManager.dematCircuit != null)
                {
                    engineManager.dematCircuit.AttemptFlight();
                }
                else
                {
                    Debug.LogWarning("TARDISMain: Engine_Demat not found for auto-dematerialization attempt.");
                }
            }
        }

        private void ControlFlight() // this method needs to be updated to use VORTEX FLIGHT.
        {
            if (engineManager == null || engineManager.navigationcom == null || consoleManager.spaceTimeThrottle == null)
            {
                Debug.LogError("TARDISMain: Missing dependencies for flight control.");
                return;
            }

            // The TARDIS will only move if it hasn't yet reached its destination.
            // Once the destination is reached, it will "hover" in the vortex.
            if (!engineManager.navigationcom.HasReachedSpatialDestination || !engineManager.navigationcom.HasReachedPocketDestination)
            {
                float normalizedThrottle = consoleManager.spaceTimeThrottle.currentThrottleValue / 11f;
                if (consoleManager.fastReturn != null && consoleManager.fastReturn.IsEngaged)
                {
                    float fastReturnSpeed = consoleManager.fastReturn.FastReturnSpeedMult;
                    float fastReturnFuel = consoleManager.fastReturn.FastReturnFuelMutl;
                    engineManager.navigationcom.FlyTowardsDestinationFastReturn(normalizedThrottle, fastReturnSpeed, fastReturnFuel);
                }
                else if (consoleManager.vortexFlight != null && consoleManager.vortexFlight.IsEngaged)
                {
                    float vortexMultiplier = consoleManager.vortexFlight.VortexMultiplier;
                    engineManager.navigationcom.FlyTowardsDestinationVortex(normalizedThrottle, vortexMultiplier);
                }
                else
                {
                    engineManager.navigationcom.FlyTowardsDestination(normalizedThrottle);
                }
            }

            // This block handles the *one-time notification* when the destination is first reached.
            // It no longer automatically triggers rematerialization.
            if (engineManager.navigationcom.HasReachedSpatialDestination && engineManager.navigationcom.HasReachedPocketDestination && !_destinationNotificationSent)
            {
                _destinationNotificationSent = true;
                NotifyDestinationReached();
            }
        }

        // --- Public Methods for TARDIS Control ---

        public void EngageFlight()
        {
            if (currentTARDISState != TARDISState.Landed)
            {
                Debug.Log("TARDIS: Cannot engage flight. Not currently landed.");
                return;
            }

            if (engineManager != null && engineManager.dematCircuit != null)
            {
                Debug.Log("TARDIS: Attempting to engage flight via Demat Circuit...");
                engineManager.dematCircuit.AttemptFlight();
            }
            else
            {
                Debug.LogError("TARDISMain: Dematerialisation Circuit not found for engaging flight!");
            }
        }

        // REMOVED: The old Rematerialize() method is now handled by Engine_Stabilisers.AttemptRematerialization()

        // --- Callbacks from Engine_Demat/NavCom to update TARDISMain's state and notify other subsystems ---
        public void SetTARDISState(TARDISState newState)
        {
            currentTARDISState = newState;
            Debug.Log($"TARDIS Main State: {currentTARDISState}");
        }

        // Callback for when NavCom reports destination reached (now only notification, no auto-remat)
        public void NotifyDestinationReached()
        {
            engineManager.soundSystem.PlayNotification(); // Play notification sound if available
            Debug.Log("TARDIS: Destination reached notification received! TARDIS is now hovering at target coordinates, awaiting manual rematerialization.");
        }

        public void NotifyDematerializationStarted()
        {
            foreach (var subsystem in GetComponentsInChildren<TARDISSubsystemController>())
            {
                subsystem.OnTARDISDematerialize();
            }
        }

        public void NotifyFlightStarted()
        {
            // Tell NavCom to record its starting position for flight progress calculation
            if (engineManager != null && engineManager.navigationcom != null)
            {
                engineManager.navigationcom.PrepareForFlight();
            }

            foreach (var subsystem in GetComponentsInChildren<TARDISSubsystemController>())
            {
                subsystem.OnTARDISFlightStart();
            }
        }

        public void NotifyMaterializationStarted()
        {
            foreach (var subsystem in GetComponentsInChildren<TARDISSubsystemController>())
            {
                subsystem.OnTARDISMaterialize();
            }
        }

        public void NotifyFlightEnded()
        {
            foreach (var subsystem in GetComponentsInChildren<TARDISSubsystemController>())
            {
                subsystem.OnTARDISFlightEnd();
            }
        }

        public void UpdateCurrentLocationsFromNavcom()
        {
            if (engineManager != null && engineManager.navigationcom != null)
            {
                currentSpatialLocation = engineManager.navigationcom.GetDestinationSpatial();
                currentPocketLocation = engineManager.navigationcom.GetDestinationPocket();
            }
        }

        // --- Internal Logic ---
        private void HandleTARDISState()
        {
            // This switch can be expanded for more complex state-specific behaviors
            switch (currentTARDISState)
            {
                case TARDISState.Landed:
                case TARDISState.Dematerializing:
                case TARDISState.Flying:
                case TARDISState.Materializing:
                case TARDISState.Danger:
                    // No specific continuous logic here for now, but provides a clear structure
                    break;
            }
        }

        public enum TARDISState
        {
            Landed,
            Dematerializing,
            Flying,
            Materializing,
            Danger // New state for crash landings or critical errors
        }

        public enum TARDISFlightState // this should be used so the TARDIS knows what flight mode it is in.
        {
            Spatial,
            Vortex,
            FastReturn
        }
    }
}