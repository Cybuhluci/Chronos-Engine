using UnityEngine;

public class Engine_Navcom : TARDISSubsystemController
{
    [Header("Dependencies")]
    [SerializeField] private TARDISMain tardisMain;
    [SerializeField] private TARDISConsoleManager consoleManager;
    [SerializeField] private TARDISEngineManager engineManager;

    [Header("Navigation Data \nCurrent navigations")]
    [SerializeField] private Vector3Int _CurrentSpatial;
    [SerializeField] private Vector3Int _CurrentPocket;

    // NEW: Store the initial coordinates when flight starts for progress calculation
    [Header("Initial Flight Coordinates")]
    [SerializeField] private Vector3Int _InitialSpatial;
    [SerializeField] private Vector3Int _InitialPocket;

    [Header("Destination nagivations")]
    [SerializeField] private Vector3Int _DestinationSpatial;
    [SerializeField] private Vector3Int _DestinationPocket;

    [Header("Flight Parameters")]
    // We'll use these to control the 'delay' between each integer step
    // IMPORTANT: Set these in the Inspector! Lower values mean faster movement.
    public float spatialStepDelay = 0.1f; // Time in seconds before moving to the next spatial coordinate
    public float pocketStepDelay = 0.1f;  // Time in seconds before moving to the next pocket coordinate

    // Internal timers for stepping
    private float _spatialTimer = 0f;
    private float _pocketTimer = 0f;

    // Property to check if the TARDIS has reached its spatial destination (exact match)
    public bool HasReachedSpatialDestination => _CurrentSpatial == _DestinationSpatial;
    // Property to check if the TARDIS has reached its pocket destination (exact match)
    public bool HasReachedPocketDestination => _CurrentPocket == _DestinationPocket;

    private void Awake()
    {
        ToggleCircuit(); // Ensure NavCom is active initially (or activated by TARDISMain)
        if (tardisMain == null) tardisMain = FindAnyObjectByType<TARDISMain>();
        if (tardisMain == null) Debug.LogError("Engine_Navcom: TARDISMain reference is missing!");
    }

    // --- TARDISSubsystemController Implementations ---
    protected override void OnCircuitActivated() { Debug.Log("Engine_Navcom: ENGAGED"); }
    protected override void OnCircuitDeactivated() { Debug.Log("Engine_Navcom: RELEASED"); }
    public override string GetCircuitStatus() { return _isCircuitActive ? "Engaged" : "Released"; }

    // --- NavCom Specific Methods ---

    // NEW: Call this when flight truly begins to store the starting point
    public void PrepareForFlight()
    {
        _InitialSpatial = _CurrentSpatial; // Capture the actual starting point
        _InitialPocket = _CurrentPocket;
        Debug.Log($"NavCom: Prepared for flight from Spatial: {_InitialSpatial}, Pocket: {_InitialPocket}");
    }

    public void SetCurrentLocation(Vector3Int spatial, Vector3Int pocket)
    {
        _CurrentSpatial = spatial;
        _CurrentPocket = pocket;
        Debug.Log($"NavCom: Current TARDIS location set to Spatial: {_CurrentSpatial}, Pocket: {_CurrentPocket}");
    }

    public void SetDestination(Vector3Int spatial, Vector3Int pocket)
    {
        _DestinationSpatial = spatial;
        _DestinationPocket = pocket;
        Debug.Log($"NavCom: Destination set to Spatial: {_DestinationSpatial}, Pocket: {_DestinationPocket}");
    }

    public Vector3Int GetCurrentSpatial() => _CurrentSpatial;
    public Vector3Int GetCurrentPocket() => _CurrentPocket;
    public Vector3Int GetDestinationSpatial() => _DestinationSpatial;
    public Vector3Int GetDestinationPocket() => _DestinationPocket;

    /// <summary>
    /// Calculates the normalized flight progress (0.0 to 1.0) based on Manhattan distance.
    /// </summary>
    public float GetFlightProgress()
    {
        // Calculate total Manhattan distance for spatial coordinates
        float totalSpatialDist =
            Mathf.Abs(_DestinationSpatial.x - _InitialSpatial.x) +
            Mathf.Abs(_DestinationSpatial.y - _InitialSpatial.y) +
            Mathf.Abs(_DestinationSpatial.z - _InitialSpatial.z);

        // Calculate current Manhattan distance covered for spatial
        float currentSpatialDistCovered =
            Mathf.Abs(_CurrentSpatial.x - _InitialSpatial.x) +
            Mathf.Abs(_CurrentSpatial.y - _InitialSpatial.y) +
            Mathf.Abs(_CurrentSpatial.z - _InitialSpatial.z);

        // Calculate total Manhattan distance for pocket coordinates
        float totalPocketDist =
            Mathf.Abs(_DestinationPocket.x - _InitialPocket.x) +
            Mathf.Abs(_DestinationPocket.y - _InitialPocket.y) +
            Mathf.Abs(_DestinationPocket.z - _InitialPocket.z);

        // Calculate current Manhattan distance covered for pocket
        float currentPocketDistCovered =
            Mathf.Abs(_CurrentPocket.x - _InitialPocket.x) +
            Mathf.Abs(_CurrentPocket.y - _InitialPocket.y) +
            Mathf.Abs(_CurrentPocket.z - _InitialPocket.z);

        float spatialProgress = (totalSpatialDist > 0) ? (currentSpatialDistCovered / totalSpatialDist) : 1f;
        float pocketProgress = (totalPocketDist > 0) ? (currentPocketDistCovered / totalPocketDist) : 1f;

        // If both journeys have a distance > 0, average their progress
        if (totalSpatialDist > 0 && totalPocketDist > 0)
        {
            return Mathf.Clamp01((spatialProgress + pocketProgress) / 2f);
        }
        else if (totalSpatialDist > 0) // Only spatial journey is active
        {
            return Mathf.Clamp01(spatialProgress);
        }
        else if (totalPocketDist > 0) // Only pocket journey is active
        {
            return Mathf.Clamp01(pocketProgress);
        }
        else
        {
            return 1f; // Both journeys have 0 total distance, meaning already at destination
        }
    }


    /// <summary>
    /// Simulates TARDIS movement towards its spatial and pocket destination while in flight.
    /// This should be called every frame while the TARDIS is in the Flying state.
    /// </summary>
    /// <param name="throttleSpeedNormalized">The current throttle speed, normalized (0.0 to 1.0).</param>
    public void FlyTowardsDestination(float throttleSpeedNormalized)
    {
        if (!isFunctional || !_isCircuitActive) return;

        if (throttleSpeedNormalized <= 0)
        {
            return;
        }

        // Adjust step delay based on throttle speed
        float currentSpatialDelay = spatialStepDelay / Mathf.Max(0.01f, throttleSpeedNormalized);
        float currentPocketDelay = pocketStepDelay / Mathf.Max(0.01f, throttleSpeedNormalized);


        // --- Spatial Movement ---
        if (!HasReachedSpatialDestination)
        {
            _spatialTimer += Time.deltaTime;
            if (_spatialTimer >= currentSpatialDelay)
            {
                // Calculate direction for each component (x, y, z)
                int dirX = _DestinationSpatial.x > _CurrentSpatial.x ? 1 : (_DestinationSpatial.x < _CurrentSpatial.x ? -1 : 0);
                int dirY = _DestinationSpatial.y > _CurrentSpatial.y ? 1 : (_DestinationSpatial.y < _CurrentSpatial.y ? -1 : 0);
                int dirZ = _DestinationSpatial.z > _CurrentSpatial.z ? 1 : (_DestinationSpatial.z < _CurrentSpatial.z ? -1 : 0);

                // Apply movement
                _CurrentSpatial.x += dirX;
                _CurrentSpatial.y += dirY;
                _CurrentSpatial.z += dirZ;

                // Clamp to ensure we don't overshoot the destination on any axis
                // This will snap the coordinate to the destination if it passes it
                if (dirX != 0) _CurrentSpatial.x = Mathf.Clamp(_CurrentSpatial.x, Mathf.Min(_DestinationSpatial.x, _CurrentSpatial.x - dirX), Mathf.Max(_DestinationSpatial.x, _CurrentSpatial.x - dirX));
                if (dirY != 0) _CurrentSpatial.y = Mathf.Clamp(_CurrentSpatial.y, Mathf.Min(_DestinationSpatial.y, _CurrentSpatial.y - dirY), Mathf.Max(_DestinationSpatial.y, _CurrentSpatial.y - dirY));
                if (dirZ != 0) _CurrentSpatial.z = Mathf.Clamp(_CurrentSpatial.z, Mathf.Min(_DestinationSpatial.z, _CurrentSpatial.z - dirZ), Mathf.Max(_DestinationSpatial.z, _CurrentSpatial.z - dirZ));

                // If we've reached the destination on this axis, ensure it's exact (redundant with clamp but good for clarity)
                if (dirX != 0 && _CurrentSpatial.x == _DestinationSpatial.x) _CurrentSpatial.x = _DestinationSpatial.x;
                if (dirY != 0 && _CurrentSpatial.y == _DestinationSpatial.y) _CurrentSpatial.y = _DestinationSpatial.y;
                if (dirZ != 0 && _CurrentSpatial.z == _DestinationSpatial.z) _CurrentSpatial.z = _DestinationSpatial.z;

                _spatialTimer = 0f; // Reset timer for the next step
            }
        }

        // --- Pocket Movement ---
        if (!HasReachedPocketDestination)
        {
            _pocketTimer += Time.deltaTime;
            if (_pocketTimer >= currentPocketDelay)
            {
                // Similar logic for pocket (assuming it's also Vector3Int or similar 3-axis concept)
                int dirPX = _DestinationPocket.x > _CurrentPocket.x ? 1 : (_DestinationPocket.x < _CurrentPocket.x ? -1 : 0);
                int dirPY = _DestinationPocket.y > _CurrentPocket.y ? 1 : (_DestinationPocket.y < _CurrentPocket.y ? -1 : 0);
                int dirPZ = _DestinationPocket.z > _CurrentPocket.z ? 1 : (_DestinationPocket.z < _CurrentPocket.z ? -1 : 0);

                _CurrentPocket.x += dirPX;
                _CurrentPocket.y += dirPY;
                _CurrentPocket.z += dirPZ;

                // Clamp to prevent overshooting
                if (dirPX != 0) _CurrentPocket.x = Mathf.Clamp(_CurrentPocket.x, Mathf.Min(_DestinationPocket.x, _CurrentPocket.x - dirPX), Mathf.Max(_DestinationPocket.x, _CurrentPocket.x - dirPX));
                if (dirPY != 0) _CurrentPocket.y = Mathf.Clamp(_CurrentPocket.y, Mathf.Min(_DestinationPocket.y, _CurrentPocket.y - dirPY), Mathf.Max(_DestinationPocket.y, _CurrentPocket.y - dirPY));
                if (dirPZ != 0) _CurrentPocket.z = Mathf.Clamp(_CurrentPocket.z, Mathf.Min(_DestinationPocket.z, _CurrentPocket.z - dirPZ), Mathf.Max(_DestinationPocket.z, _CurrentPocket.z - dirPZ));

                // Ensure exact match if reached
                if (dirPX != 0 && _CurrentPocket.x == _DestinationPocket.x) _CurrentPocket.x = _DestinationPocket.x;
                if (dirPY != 0 && _CurrentPocket.y == _DestinationPocket.y) _CurrentPocket.y = _DestinationPocket.y;
                if (dirPZ != 0 && _CurrentPocket.z == _DestinationPocket.z) _CurrentPocket.z = _DestinationPocket.z;

                _pocketTimer = 0f; // Reset timer
            }
        }

        // Check if destination is reached (now uses exact equality)
        if (HasReachedSpatialDestination && HasReachedPocketDestination)
        {
            Debug.Log("NavCom: Destination reached! Notifying TARDISMain for rematerialization.");
            tardisMain.NotifyDestinationReached();
        }
    }
}