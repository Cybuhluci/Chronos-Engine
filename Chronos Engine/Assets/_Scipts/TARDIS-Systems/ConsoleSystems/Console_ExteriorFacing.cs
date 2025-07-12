using UnityEngine;
using Luci.TARDIS;
using Luci.TARDIS.Engine;

namespace Luci.TARDIS.Console
{
    public class Console_ExteriorFacing : TARDISSubsystemController
    {
        [Header("Dependencies")]
        [SerializeField] private TARDISMain tardisMain;
        [SerializeField] private TARDISConsoleManager consoleManager;
        [SerializeField] private TARDISEngineManager engineManager;
        public bool IsEngaged => _isCircuitActive;
        // --- TARDISSubsystemController Implementations ---
        // This method is called by the base ToggleCircuit() when _isCircuitActive becomes TRUE.
        protected override void OnCircuitActivated()
        {
            Debug.Log("Console_ExteriorFacing: ENGAGED");
        }
        // This method is called by the base ToggleCircuit() when _isCircuitActive becomes FALSE.
        protected override void OnCircuitDeactivated()
        {
            Debug.Log("Console_ExteriorFacing: DISENGAGED");
        }
        // This method provides the current status string, using the base class's _isCircuitActive.
        public override string GetCircuitStatus()
        {
            return _isCircuitActive ? "Engaged" : "Released";
        }
    }
}
