using UnityEngine;
using Luci.TARDIS;
using Luci.TARDIS.Engine;

namespace Luci.TARDIS.Console
{
    public class Console_CoordinateControl : TARDISSubsystemController
    {
        [Header("Dependencies")]
        [SerializeField] private TARDISMain tardisMain;
        [SerializeField] private TARDISConsoleManager consoleManager;
        [SerializeField] private TARDISEngineManager engineManager;
        public bool IsEngaged => _isCircuitActive;

        [Header("Adjustment Settings")]
        public int[] incrementAmounts = { 1, 10, 50, 100 }; // The different increment sizes
        private int _currentIncrementIndex = 0;
        public int _selectedIncrementAmount = 1; // Starts at 1

        // NEW: Flag to store the current adjustment direction (true = positive, false = negative)
        public bool _isIncrementDirectionPositive = true;

        // We still keep this enum here incase the new system needs it.
        private enum SelectedCoordinate { None, ClusterPlot, GalaxyPlot, PlanetPlot, PocketPlot }
        private SelectedCoordinate _lastAdjustedCoordinate = SelectedCoordinate.None; // Track last for status display

        // --- TARDISSubsystemController Implementations ---
        // This method is called by the base ToggleCircuit() when _isCircuitActive becomes TRUE.
        protected override void OnCircuitActivated()
        {
            Debug.Log("Console_CoordinateControl: ENGAGED");
        }
        // This method is called by the base ToggleCircuit() when _isCircuitActive becomes FALSE.
        protected override void OnCircuitDeactivated()
        {
            Debug.Log("Console_CoordinateControl: DISENGAGED");
        }
        // This method provides the current status string, using the base class's _isCircuitActive.
        public override string GetCircuitStatus()
        {
            return _isCircuitActive ? "Engaged" : "Released";
        }

        public void increaseWCoordinate()
        {

        }

        public void decreaseWCoordinate()
        {
        
        }

        public void increaseXCoordinate()
        {

        }

        public void decreaseXCoordinate()
        {

        }

        public void increaseYCoordinate()
        {

        }

        public void decreaseYCoordinate()
        {

        }

        public void increaseZCoordinate()
        {

        }

        public void decreaseZCoordinate()
        {

        }
    }
}