using UnityEngine;
using Luci.TARDIS;
using Luci.TARDIS.Engine;

namespace Luci.TARDIS.Console
{
    public class Console_VortexFlight : TARDISSubsystemController
    {
        [Header("Dependencies")]
        [SerializeField] private TARDISMain tardisMain;
        [SerializeField] private TARDISConsoleManager consoleManager;
        [SerializeField] private TARDISEngineManager engineManager;
        public bool IsEngaged => _isCircuitActive;
        public float VortexMultiplier = 2.0f; // Multiplier for vortex flight speed

        // --- TARDISSubsystemController Implementations ---

        // This method is called by the base ToggleCircuit() when _isCircuitActive becomes TRUE.
        protected override void OnCircuitActivated()
        {
            Debug.Log("Console_VortexFlight: ENGAGED");
        }

        // This method is called by the base ToggleCircuit() when _isCircuitActive becomes FALSE.
        protected override void OnCircuitDeactivated()
        {
            Debug.Log("Console_VortexFlight: DISENGAGED");
        }

        // This method provides the current status string, using the base class's _isCircuitActive.
        public override string GetCircuitStatus()
        {
            return _isCircuitActive ? "Engaged" : "Released";
        }
    }
}