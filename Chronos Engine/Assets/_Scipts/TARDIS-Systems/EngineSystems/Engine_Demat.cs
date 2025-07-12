using UnityEngine;
using System.Collections; // Required for Coroutines
using Luci.TARDIS;
using Luci.TARDIS.Console;

namespace Luci.TARDIS.Engine
{
    public class Engine_Demat : TARDISSubsystemController
    {
        [Header("Dependencies")]
        [SerializeField] private TARDISMain tardisMain;
        [SerializeField] private TARDISConsoleManager consoleManager; // Now a direct dependency
        [SerializeField] private TARDISEngineManager engineManager;
        [SerializeField] private Luci.TARDIS.EngineSystems.Engine_SoundSystem soundSystem;

        [Header("Flight Parameters")]
        public float dematerializationDuration = 5f; // How long demat takes
        public float materializationDuration = 5f; // How long mat takes

        // Internal coroutine reference to manage ongoing sequences
        private Coroutine _currentDematSequence;

        private void Awake()
        {
            ToggleCircuit();

            // Ensure we get references for our dependencies
            if (tardisMain == null) tardisMain = FindAnyObjectByType<TARDISMain>();
            if (engineManager == null) engineManager = GetComponentInParent<TARDISEngineManager>();
            if (soundSystem == null) soundSystem = FindAnyObjectByType<Luci.TARDIS.EngineSystems.Engine_SoundSystem>();

            if (tardisMain != null && consoleManager == null)
            {
                consoleManager = tardisMain.consoleManager;
            }

            // Error checks
            if (tardisMain == null) Debug.LogError("Engine_Demat: TARDISMain reference is missing!");
            if (engineManager == null) Debug.LogError("Engine_Demat: TARDISEngineManager reference is missing!");
            if (consoleManager == null) Debug.LogError("Engine_Demat: TARDISConsoleManager reference is missing! Cannot check console controls.");
            if (soundSystem == null) Debug.LogWarning("Engine_Demat: Engine_SoundSystem reference is missing! Sounds will not play.");
        }

        // --- TARDISSubsystemController Implementations (Keep as is for now) ---
        protected override void OnCircuitActivated() { Debug.Log("Engine_Demat: Circuit ENGAGED"); }
        protected override void OnCircuitDeactivated()
        {
            Debug.Log("Engine_Demat: Circuit RELEASED");
            if (_currentDematSequence != null)
            {
                StopCoroutine(_currentDematSequence);
                _currentDematSequence = null;
                if (tardisMain != null) tardisMain.SetTARDISState(TARDISMain.TARDISState.Landed); // Force land
                if (soundSystem != null) {
                    soundSystem.StopFlightLoop();
                }
                Debug.Log("Engine_Demat: Aborting flight sequence due to circuit deactivation!");
            }
        }
        public override string GetCircuitStatus() { return _isCircuitActive ? "Engaged" : "Released"; }

        // --- NEW: Public method for TARDISMain to call to attempt initiating flight ---
        public void AttemptFlight()
        {
            if (!isFunctional || !_isCircuitActive)
            {
                Debug.Log("Engine_Demat: Dematerialisation Circuit not functional or active. Cannot attempt flight.");
                return;
            }

            if (consoleManager == null || engineManager == null ||
                consoleManager.timeRotorHandbrake == null || engineManager.navigationcom == null ||
                consoleManager.spaceTimeThrottle == null)
            {
                Debug.LogError("Engine_Demat: Missing essential console/engine components for flight. Cannot launch.");
                return;
            }

            // --- Check both Telepathic and Delta circuits for valid, operational input ---
            Vector3Int targetSpatial = Vector3Int.zero;
            Vector3Int targetPocket = Vector3Int.zero;
            bool circuitReady = false;

            // Prefer Telepathic Circuit if both are valid
            if (consoleManager.telepathicCircuit != null && consoleManager.telepathicCircuit.IsFullyOperational())
            {
                var destS = consoleManager.telepathicCircuit.GetDestination();
                var destP = consoleManager.telepathicCircuit.GetPocketDestination();
                if (destS != Vector3Int.zero || destP != Vector3Int.zero)
                {
                    targetSpatial = destS;
                    targetPocket = destP;
                    circuitReady = true;
                }
            }
            // If telepathic not ready, try Delta Circuit
            if (!circuitReady && consoleManager.deltaCircuit != null && consoleManager.deltaCircuit.IsFullyOperational())
            {
                var destS = consoleManager.deltaCircuit.targetSpatialCoordinates;
                var destP = consoleManager.deltaCircuit.targetPocketCoordinates;
                if (destS != Vector3Int.zero || destP != Vector3Int.zero)
                {
                    targetSpatial = destS;
                    targetPocket = destP;
                    circuitReady = true;
                }
            }

            if (!circuitReady)
            {
                Debug.LogWarning("Engine_Demat: No valid destination set in any operational circuit. Cannot dematerialize.");
                return;
            }

            bool handbrakeOff = !consoleManager.timeRotorHandbrake.IsEngaged;
            bool throttleActiveAndPulled = consoleManager.spaceTimeThrottle.IsCircuitActive && consoleManager.spaceTimeThrottle.currentThrottleValue > 0;
            bool allCircuitsReady = engineManager.navigationcom.IsFullyOperational() && consoleManager.spaceTimeThrottle.IsFullyOperational();

            if (handbrakeOff && throttleActiveAndPulled && allCircuitsReady)
            {
                Debug.Log($"Engine_Demat: All conditions met! Initiating dematerialization sequence to Spatial: {targetSpatial}, Pocket: {targetPocket}");

                if (_currentDematSequence != null) StopCoroutine(_currentDematSequence);
                _currentDematSequence = StartCoroutine(DematerializationSequence(targetSpatial, targetPocket));
            }
            else
            {
                string reason = "Engine_Demat: Flight conditions not met: ";
                if (!handbrakeOff) reason += "Handbrake engaged. ";
                if (!throttleActiveAndPulled) reason += "Throttle not active or pulled. ";
                if (!allCircuitsReady) reason += "Not all required circuits are ready. ";
                Debug.LogWarning(reason);
            }
        }

        // Materialization is now triggered via TARDISMain's Rematerialize()
        // and this method itself is now private and part of the sequence.
        private IEnumerator MaterializationSequence() // Changed to private
        {
            tardisMain.SetTARDISState(TARDISMain.TARDISState.Materializing);
            tardisMain.NotifyMaterializationStarted();

            Debug.Log("TARDIS: Materializing...");

            if (soundSystem != null) {
                soundSystem.StopFlightLoop();
                soundSystem.PlayMaterialization();
            }

            yield return new WaitForSeconds(materializationDuration);

            tardisMain.SetTARDISState(TARDISMain.TARDISState.Landed);
            tardisMain.UpdateCurrentLocationsFromNavcom();
            tardisMain.NotifyFlightEnded();

            Debug.Log($"TARDIS: Successfully materialized at Spatial: {tardisMain.currentSpatialLocation}, Chronal: {tardisMain.currentPocketLocation}");

            _currentDematSequence = null;
        }

        // --- Coroutines for TARDIS Sequences ---
        // This coroutine still takes parameters, but these parameters will now
        // come directly from AttemptFlight() after it has validated them.
        private IEnumerator DematerializationSequence(Vector3Int targetSpatial, Vector3Int targetPocket)
        {
            tardisMain.SetTARDISState(TARDISMain.TARDISState.Dematerializing);
            tardisMain.NotifyDematerializationStarted();

            Debug.Log("TARDIS: Dematerializing...");

            if (soundSystem != null) {
                soundSystem.PlayDematerialization();
            }

            yield return new WaitForSeconds(dematerializationDuration);

            // Update NavCom with the TARDIS's *actual* current location (where it dematerialized from)
            // and set the *destination* for the flight.
            if (engineManager != null && engineManager.navigationcom != null)
            {
                engineManager.navigationcom.SetCurrentLocation(tardisMain.currentSpatialLocation, tardisMain.currentPocketLocation);
                engineManager.navigationcom.SetDestination(targetSpatial, targetPocket);
            }

            tardisMain.SetTARDISState(TARDISMain.TARDISState.Flying);
            tardisMain.NotifyFlightStarted();

            Debug.Log($"TARDIS: Now in Vortex Flight towards Spatial: {targetSpatial}, Chronal: {targetPocket}");

            if (soundSystem != null) {
                soundSystem.PlayFlightLoop();
            }
        }

        // Public method to start materialization, called by TARDISMain
        public void StartMaterializationSequenceFromMain()
        {
            if (!isFunctional || !_isCircuitActive)
            {
                Debug.Log("Engine_Demat: Dematerialisation Circuit not functional or active. Cannot start materialization.");
                return;
            }

            if (_currentDematSequence != null) StopCoroutine(_currentDematSequence);
            _currentDematSequence = StartCoroutine(MaterializationSequence());
        }
    }
}