using UnityEngine;
using Luci.TARDIS;
using Luci.TARDIS.Console;

namespace Luci.TARDIS.Engine
{
    public class Engine_Chameleon : TARDISSubsystemController
    {
        [Header("Dependencies")]
        [SerializeField] private TARDISMain tardisMain;
        [SerializeField] private TARDISConsoleManager consoleManager;
        [SerializeField] private TARDISEngineManager engineManager;

        // --- TARDISSubsystemController Implementations ---
        protected override void OnCircuitActivated() { Debug.Log("Engine_Chameleon: ENGAGED"); }
        protected override void OnCircuitDeactivated() { Debug.Log("Engine_Chameleon: RELEASED"); }
        public override string GetCircuitStatus() { return _isCircuitActive ? "Engaged" : "Released"; }
    }
}
