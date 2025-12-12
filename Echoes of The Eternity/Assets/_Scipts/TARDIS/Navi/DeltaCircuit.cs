using TARDIS.Core;
using Unity.Mathematics;
using UnityEngine;

public class DeltaCircuit : ConsoleCore
{
    // --- TARDISSubsystemController Implementations ---

    // This method is called by the base ToggleCircuit() when _isCircuitActive becomes TRUE.
    protected override void OnCircuitActivated() { }
    // This method is called by the base ToggleCircuit() when _isCircuitActive becomes FALSE.
    protected override void OnCircuitDeactivated() { }

    // --- Coordinate Adjustment Methods for Physical Buttons ---

    public NaviCore naviCore; // Reference to the NaviCore to update target coordinates

    [Header("Current Destination")]
    public int4 targetCoordinates;
    public int sectorCoordinates; // 1-8, mimics a 2x2x2 vector3
    public Vector3 majorCoordinates; // 250x250x250
    public Vector3 minorCoordinates; // 500x500x500

    [Header("Adjustment Settings")]
    public int[] incrementAmounts = { 1, 10, 50, 100 }; // The different increment sizes
    private int _currentIncrementIndex = 0;
    public int _selectedIncrementAmount = 1; // Starts at 1

    // NEW: Flag to store the current adjustment direction (true = positive, false = negative)
    public bool isIncrementDirectionPositive = true;

    // We still keep this enum here incase the new system needs it.
    private enum SelectedCoordinate { None, ClusterPlot, GalaxyPlot, PlanetPlot, PocketPlot }

    private void Awake()
    {
        ToggleCircuit();
    }
}