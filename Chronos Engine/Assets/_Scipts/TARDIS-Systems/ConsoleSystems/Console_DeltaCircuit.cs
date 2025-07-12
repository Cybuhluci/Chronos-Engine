using UnityEngine;
using Luci.TARDIS;
using Luci.TARDIS.Engine;

namespace Luci.TARDIS.Console
{
    public class Console_DeltaCircuit : TARDISSubsystemController
    {
        [Header("Dependencies")]
        [SerializeField] private TARDISMain tardisMain;
        [SerializeField] private TARDISConsoleManager consoleManager;
        [SerializeField] private TARDISEngineManager engineManager;
        public bool IsEngaged => _isCircuitActive;

        [Header("Current Destination")]
        public Vector3Int targetSpatialCoordinates;
        public Vector3Int targetPocketCoordinates;

        [Header("Adjustment Settings")]
        public int[] incrementAmounts = { 1, 10, 50, 100 }; // The different increment sizes
        private int _currentIncrementIndex = 0;
        public int _selectedIncrementAmount = 1; // Starts at 1

        // NEW: Flag to store the current adjustment direction (true = positive, false = negative)
        public bool isIncrementDirectionPositive = true;

        // We still keep this enum here incase the new system needs it.
        private enum SelectedCoordinate { None, ClusterPlot, GalaxyPlot, PlanetPlot, PocketPlot }
        private SelectedCoordinate _lastAdjustedCoordinate = SelectedCoordinate.None; // Track last for status display

        private void Awake()
        {
            ToggleCircuit();
        }

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

        // --- Coordinate Adjustment Methods for Physical Buttons ---
        public void increaseWCoordinate() { AdjustPocketCoordinate(1); }
        public void decreaseWCoordinate() { AdjustPocketCoordinate(-1); }
        public void increaseXCoordinate() { AdjustSpatialCoordinate(0, 1); }
        public void decreaseXCoordinate() { AdjustSpatialCoordinate(0, -1); }
        public void increaseYCoordinate() { AdjustSpatialCoordinate(1, 1); }
        public void decreaseYCoordinate() { AdjustSpatialCoordinate(1, -1); }
        public void increaseZCoordinate() { AdjustSpatialCoordinate(2, 1); }
        public void decreaseZCoordinate() { AdjustSpatialCoordinate(2, -1); }

        // Helper methods
        private void AdjustSpatialCoordinate(int axis, int direction)
        {
            int adjustment = _selectedIncrementAmount * direction * (isIncrementDirectionPositive ? 1 : -1);
            switch (axis)
            {
                case 0: targetSpatialCoordinates.x += adjustment; break;
                case 1: targetSpatialCoordinates.y += adjustment; break;
                case 2: targetSpatialCoordinates.z += adjustment; break;
            }
            UpdateNavcom();
        }

        private void AdjustPocketCoordinate(int direction)
        {
            int adjustment =  direction * (isIncrementDirectionPositive ? 1 : -1);
            targetPocketCoordinates.x += adjustment;
            targetPocketCoordinates.x = Mathf.Clamp(targetPocketCoordinates.x, 1, 9);
            UpdateNavcom();
        }

        private void UpdateNavcom()
        {
            if (engineManager != null && engineManager.navigationcom != null)
            {
                engineManager.navigationcom.SetDestination(targetSpatialCoordinates, targetPocketCoordinates);
            }
        }

        // Optionally, add methods to cycle increment and toggle direction for physical controls
        public void CycleIncrementAmountUp()
        {
            _currentIncrementIndex = (_currentIncrementIndex + 1) % incrementAmounts.Length;
            _selectedIncrementAmount = incrementAmounts[_currentIncrementIndex];
        }

        public void CycleIncrementAmountDown()
        {
            _currentIncrementIndex = (_currentIncrementIndex - 1 + incrementAmounts.Length) % incrementAmounts.Length;
            _selectedIncrementAmount = incrementAmounts[_currentIncrementIndex];
        }
    }
}