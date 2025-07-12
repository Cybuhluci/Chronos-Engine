using UnityEngine;
using System.Collections; // Required for Coroutines
using Luci.TARDIS;
using Luci.TARDIS.Console;

namespace Luci.TARDIS.Engine
{

    public class Engine_Stabilisers : TARDISSubsystemController // Changed from MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private TARDISMain tardisMain;
        [SerializeField] private TARDISConsoleManager consoleManager;
        [SerializeField] private TARDISEngineManager engineManager; // Contains Navcom and Demat references

        // NEW: References to the Console_Throttle and Console_Handbrake
        [SerializeField] private Console_Throttle consoleThrottle;
        [SerializeField] private Console_Handbrake consoleHandbrake;

        // A flag to prevent multiple rematerialization attempts while one is in progress
        private bool _isRematerializing = false;

        // --- TARDISSubsystemController Implementations ---
        protected override void OnCircuitActivated() { Debug.Log("Engine_Stabilisers: ACTIVE"); }
        protected override void OnCircuitDeactivated() { Debug.Log("Engine_Stabilisers: INACTIVE"); }
        public override string GetCircuitStatus() { return _isCircuitActive ? "Active" : "Inactive"; }

        private void Awake()
        {
            // Find dependencies if not set in Inspector (good for robust initialization)
            if (tardisMain == null) tardisMain = FindAnyObjectByType<TARDISMain>();
            if (consoleManager == null && tardisMain != null) consoleManager = tardisMain.consoleManager;
            if (engineManager == null && tardisMain != null) engineManager = tardisMain.engineManager;
            if (consoleThrottle == null) consoleThrottle = FindAnyObjectByType<Console_Throttle>();
            if (consoleHandbrake == null) consoleHandbrake = FindAnyObjectByType<Console_Handbrake>();

            // Basic error checking for critical dependencies
            if (tardisMain == null || consoleManager == null || engineManager == null || consoleThrottle == null || consoleHandbrake == null)
            {
                Debug.LogError("Engine_Stabilisers: Missing critical dependencies. Please assign in Inspector or ensure they exist in scene.");
            }
        }

        /// <summary>
        /// Attempts to rematerialize the TARDIS based on current conditions.
        /// This is called when the Handbrake is toggled or when Throttle drops to 0 with Handbrake engaged.
        /// </summary>
        public void AttemptRematerialization()
        {
            // Prevent new attempts if already in a rematerialization sequence
            if (_isRematerializing)
            {
                Debug.Log("Engine_Stabilisers: Rematerialization already in progress. Ignoring new attempt.");
                return;
            }

            // Only attempt remat if TARDIS is currently flying
            if (tardisMain.currentTARDISState != TARDISMain.TARDISState.Flying)
            {
                Debug.Log("Engine_Stabilisers: Cannot rematerialize. TARDIS is not currently in flight.");
                return;
            }

            // Ensure NavCom has actually reached the destination coordinates
            if (engineManager.navigationcom == null || !engineManager.navigationcom.HasReachedSpatialDestination || !engineManager.navigationcom.HasReachedPocketDestination)
            {
                Debug.Log("Engine_Stabilisers: Cannot rematerialize. Destination has not been reached yet.");
                // Optional: Play a 'still moving' sound or display a message to the player.
                return;
            }

            // Get current states of throttle and handbrake
            bool throttleAtZero = consoleThrottle.currentThrottleValue == 0;
            bool handbrakeEngaged = consoleHandbrake.IsEngaged;

            // --- Determine Rematerialization Type ---
            if (throttleAtZero && handbrakeEngaged)
            {
                // SAFE REMATERIALIZATION
                Debug.Log("Engine_Stabilisers: Initiating SAFE rematerialization (Throttle 0, Handbrake engaged).");
                StartCoroutine(SafeRematerializeSequence());
            }
            else if (handbrakeEngaged && consoleThrottle.currentThrottleValue > 0)
            {
                // CRASH REMATERIALIZATION
                Debug.LogWarning("Engine_Stabilisers: Initiating CRASH rematerialization! Handbrake pulled while throttle is active!");
                StartCoroutine(CrashRematerializeSequence());
            }
            else
            {
                // Conditions not met for either safe or crash (e.g., handbrake is not engaged)
                Debug.Log("Engine_Stabilisers: Rematerialization conditions not met. Ensure throttle is 0 and handbrake is engaged for a safe landing, or pull handbrake while moving for a crash.");
            }
        }

        private IEnumerator SafeRematerializeSequence()
        {
            _isRematerializing = true;

            // Force throttle to 0 and ensure handbrake is engaged
            consoleThrottle.SetThrottle(0);
            if (!consoleHandbrake.IsEngaged) consoleHandbrake.ToggleCircuit(); // Engage handbrake if it's not already

            // Add visual and audio feedback for a smooth, safe landing here
            Debug.Log("Stabilisers: Performing safe landing sequence...");
            // Example: Play a gentle hum, subtle screen fade, minor console hums.
            yield return new WaitForSeconds(0.5f); // Short delay for atmosphere

            // Tell the Demat Circuit to start the materialization process
            if (engineManager.dematCircuit != null)
            {
                engineManager.dematCircuit.StartMaterializationSequenceFromMain();
            }
            else
            {
                Debug.LogError("Engine_Stabilisers: Dematerialisation Circuit not found for safe rematerialization!");
            }

            // Wait until the TARDIS state confirms it has landed
            yield return new WaitUntil(() => tardisMain.currentTARDISState == TARDISMain.TARDISState.Landed);

            _isRematerializing = false;
            Debug.Log("Stabilisers: Safe landing complete!");
        }

        private IEnumerator CrashRematerializeSequence()
        {
            _isRematerializing = true;

            // Force handbrake engaged (it should be, but for safety)
            if (!consoleHandbrake.IsEngaged) consoleHandbrake.ToggleCircuit();
            // Throttle remains as is for a crash landing (active)

            // Add dramatic visual and audio feedback for a crash landing
            Debug.LogWarning("Stabilisers: CRASH LANDING! Brace for impact!");
            // Example: Loud bang, violent screen shake, alarms, console lights flickering.
            tardisMain.SetTARDISState(TARDISMain.TARDISState.Danger); // Set TARDIS state to Danger
            yield return new WaitForSeconds(1.5f); // Longer, more chaotic impact simulation

            // Tell the Demat Circuit to forcefully materialize
            if (engineManager.dematCircuit != null)
            {
                // You might have a specific materialization sequence for crashes (e.g., faster, glitchy, no sound effects)
                engineManager.dematCircuit.StartMaterializationSequenceFromMain();
            }
            else
            {
                Debug.LogError("Engine_Stabilisers: Dematerialisation Circuit not found for crash rematerialization!");
            }

            // Wait until the TARDIS state confirms it has landed (even if it's a crash landing)
            yield return new WaitUntil(() => tardisMain.currentTARDISState == TARDISMain.TARDISState.Landed);

            _isRematerializing = false;
            Debug.Log("Stabilisers: Crash landing complete!");
            // Implement further crash consequences here (e.g., damage subsystems, require repairs, affect interior)
        }

        // --- FUTURE: Auto-Remat Logic (can be triggered by NavCom reaching destination) ---
        // You could call this from TARDISMain.NotifyDestinationReached() if auto-remat is enabled.
        // public void OnNavComDestinationReachedForAutoStabilisation()
        // {
        //     // Example conditions for auto-remat:
        //     if (autoRematerializeEnabled && _isCircuitActive && tardisMain.currentTARDISState == TARDISState.Flying && consoleThrottle.currentThrottleValue == 0)
        //     {
        //         Debug.Log("Stabilisers: Auto-rematerialization conditions met. Initiating safe landing.");
        //         StartCoroutine(SafeRematerializeSequence());
        //     }
        // }
    }
}