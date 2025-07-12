using UnityEngine;
using Luci.TARDIS;
using Luci.TARDIS.Engine;

namespace Luci.TARDIS.Console
{
    public class Console_FastReturn : TARDISSubsystemController
    {
        [Header("Dependencies")]
        [SerializeField] private TARDISMain tardisMain;
        [SerializeField] private TARDISConsoleManager consoleManager;
        [SerializeField] private TARDISEngineManager engineManager;
        public bool IsEngaged => _isCircuitActive;
        public float FastReturnSpeedMult = 2.0f; // Multiplier for fast return flight speed
        public float FastReturnFuelMutl = 3f; // Multiplier for fuel consumption during fast return

        // --- TARDISSubsystemController Implementations ---

        // This method is called by the base ToggleCircuit() when _isCircuitActive becomes TRUE.
        protected override void OnCircuitActivated()
        {
            Debug.Log("Console_FastReturn: ENGAGED");
        }

        // This method is called by the base ToggleCircuit() when _isCircuitActive becomes FALSE.
        protected override void OnCircuitDeactivated()
        {
            Debug.Log("Console_FastReturn: DISENGAGED");
        }

        // This method provides the current status string, using the base class's _isCircuitActive.
        public override string GetCircuitStatus()
        {
            return _isCircuitActive ? "Engaged" : "Released";
        }
    }
}