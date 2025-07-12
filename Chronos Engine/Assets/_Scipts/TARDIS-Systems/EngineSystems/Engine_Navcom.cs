using UnityEngine;
using Luci.TARDIS;
using Luci.TARDIS.Console;

namespace Luci.TARDIS.Engine
{
    public class Engine_Navcom : TARDISSubsystemController
    {
        [Header("Dependencies")]
        [SerializeField] private TARDISMain tardisMain;
        [SerializeField] private TARDISConsoleManager consoleManager;
        [SerializeField] private TARDISEngineManager engineManager;

        [Header("Navigation Data \nCurrent navigations")]
        [SerializeField] private Vector3Int _CurrentSpatial;
        [SerializeField] private Vector3Int _CurrentPocket;
        [SerializeField] private Vector3Int _LastSpatial;
        [SerializeField] private Vector3Int _LastPocket;

        // NEW: Store the initial coordinates when flight starts for progress calculation
        [Header("Initial Flight Coordinates")]
        [SerializeField] private Vector3Int _InitialSpatial;
        [SerializeField] private Vector3Int _InitialPocket;

        [Header("Destination nagivations")]
        [SerializeField] private Vector3Int _DestinationSpatial;
        [SerializeField] private Vector3Int _DestinationPocket;

        [Header("Vortex nagivations")]
        [SerializeField] private Vector3Int _CurrentVortexPosition;
        [SerializeField] private Vector3Int _CurrentVortexPocket;

        [Header("Flight Parameters")]
        // We'll use these to control the 'delay' between each integer step
        public float spatialStepDelay = 0.1f;
        public float pocketStepDelay = 0.1f;
        // NEW VERSION: for flight parameters
        // new systems needs to calculate the distance travelled in the 3d vector space
        // this would need to do stuff to figure out 50 grid movements even when moving diagonally
        public float ArtronPerStep = 0.5f; // Amount of Artron energy consumed per step
        public int GridspacesPerStep = 50; // how many gridspaces moved count as a step
        private float _spatialDistanceAccumulator = 0f;
        public int StepTime = 60; // How many seconds it takes to complete a step at throttle 11

        // Internal timers for stepping
        private float _spatialTimer = 0f;
        private float _pocketTimer = 0f;

        // For smooth interpolation
        private Vector3 _stepStartSpatial;
        private Vector3 _stepEndSpatial;
        private bool _isStepInProgress = false;

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
            //Debug.Log($"NavCom: Destination set to Spatial: {_DestinationSpatial}, Pocket: {_DestinationPocket}");
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
        public Vector3 GetInterpolatedSpatial()
        {
            if (!_isStepInProgress || StepTime <= 0f)
                return _CurrentSpatial;
            float t = Mathf.Clamp01(_spatialTimer / StepTime);
            return Vector3.Lerp(_stepStartSpatial, _stepEndSpatial, t);
        }

        public void FlyTowardsDestination(float throttleSpeedNormalized)
        {
            if (!isFunctional || !_isCircuitActive) return;
            if (throttleSpeedNormalized <= 0) return;

            var fluidLink = engineManager != null ? engineManager.fluidlinks : null;
            if (fluidLink == null)
            {
                Debug.LogWarning("Engine_Navcom: FluidLink not found, cannot consume Artron Energy.");
                return;
            }

            // --- Spatial Movement (per-gridspace, time-based, visible increments) ---
            if (!HasReachedSpatialDestination)
            {
                float timePerGridspace = (StepTime / (float)GridspacesPerStep) / Mathf.Max(throttleSpeedNormalized, 0.01f);
                _spatialTimer += Time.deltaTime;
                while (_spatialTimer >= timePerGridspace && !HasReachedSpatialDestination)
                {
                    // Move one gridspace toward the destination
                    int dirX = _DestinationSpatial.x > _CurrentSpatial.x ? 1 : (_DestinationSpatial.x < _CurrentSpatial.x ? -1 : 0);
                    int dirY = _DestinationSpatial.y > _CurrentSpatial.y ? 1 : (_DestinationSpatial.y < _CurrentSpatial.y ? -1 : 0);
                    int dirZ = _DestinationSpatial.z > _CurrentSpatial.z ? 1 : (_DestinationSpatial.z < _CurrentSpatial.z ? -1 : 0);

                    if (dirX == 0 && dirY == 0 && dirZ == 0)
                        break;

                    _CurrentSpatial.x += dirX;
                    _CurrentSpatial.y += dirY;
                    _CurrentSpatial.z += dirZ;

                    // Consume a fraction of fuel per gridspace
                    float fuelPerGridspace = ArtronPerStep / Mathf.Max(GridspacesPerStep, 1);
                    if (fluidLink.FuelLeft < fuelPerGridspace)
                    {
                        Debug.LogWarning("Engine_Navcom: Not enough Artron Energy to continue movement!");
                        //engineManager.stabilisers.forcecrashlanding();
                        return;
                    }
                    fluidLink.FuelLeft -= fuelPerGridspace;
                    _spatialDistanceAccumulator += 1f;
                    _spatialTimer -= timePerGridspace;
                }
            }
            // --- Pocket Movement (unchanged) ---
            if (!HasReachedPocketDestination)
            {
                _pocketTimer += Time.deltaTime;
                if (_pocketTimer >= pocketStepDelay)
                {
                    int dirPX = _DestinationPocket.x > _CurrentPocket.x ? 1 : (_DestinationPocket.x < _CurrentPocket.x ? -1 : 0);
                    int dirPY = _DestinationPocket.y > _CurrentPocket.y ? 1 : (_DestinationPocket.y < _CurrentPocket.y ? -1 : 0);
                    int dirPZ = _DestinationPocket.z > _CurrentPocket.z ? 1 : (_DestinationPocket.z < _CurrentPocket.z ? -1 : 0);

                    _CurrentPocket.x += dirPX;
                    _CurrentPocket.y += dirPY;
                    _CurrentPocket.z += dirPZ;

                    if (dirPX != 0) _CurrentPocket.x = Mathf.Clamp(_CurrentPocket.x, Mathf.Min(_DestinationPocket.x, _CurrentPocket.x - dirPX), Mathf.Max(_DestinationPocket.x, _CurrentPocket.x - dirPX));
                    if (dirPY != 0) _CurrentPocket.y = Mathf.Clamp(_CurrentPocket.y, Mathf.Min(_DestinationPocket.y, _CurrentPocket.y - dirPY), Mathf.Max(_DestinationPocket.y, _CurrentPocket.y - dirPY));
                    if (dirPZ != 0) _CurrentPocket.z = Mathf.Clamp(_CurrentPocket.z, Mathf.Min(_DestinationPocket.z, _CurrentPocket.z - dirPZ), Mathf.Max(_DestinationPocket.z, _CurrentPocket.z - dirPZ));

                    if (dirPX != 0 && _CurrentPocket.x == _DestinationPocket.x) _CurrentPocket.x = _DestinationPocket.x;
                    if (dirPY != 0 && _CurrentPocket.y == _DestinationPocket.y) _CurrentPocket.y = _DestinationPocket.y;
                    if (dirPZ != 0 && _CurrentPocket.z == _DestinationPocket.z) _CurrentPocket.z = _DestinationPocket.z;

                    _pocketTimer = 0f;
                }
            }

            // Check if destination is reached (now uses exact equality)
            if (HasReachedSpatialDestination && HasReachedPocketDestination)
            {
                Debug.Log("NavCom: Destination reached! Notifying TARDISMain for rematerialization.");
                tardisMain.NotifyDestinationReached();
            }
        }

        public void FlyTowardsDestinationVortex(float throttleSpeedNormalized, float vortexMultiplier)
        {
            if (!isFunctional || !_isCircuitActive) return;
            if (throttleSpeedNormalized <= 0) return;

            var fluidLink = engineManager != null ? engineManager.fluidlinks : null;
            if (fluidLink == null)
            {
                Debug.LogWarning("Engine_Navcom: FluidLink not found, cannot consume Artron Energy.");
                return;
            }

            // --- Vortex Spatial Movement (uses vortexMultiplier for speed) ---
            if (!HasReachedSpatialDestination)
            {
                float timePerGridspace = ((StepTime / (float)GridspacesPerStep) / Mathf.Max(throttleSpeedNormalized, 0.01f)) / Mathf.Max(vortexMultiplier, 0.01f);
                _spatialTimer += Time.deltaTime;
                while (_spatialTimer >= timePerGridspace && !HasReachedSpatialDestination)
                {
                    int dirX = _DestinationSpatial.x > _CurrentSpatial.x ? 1 : (_DestinationSpatial.x < _CurrentSpatial.x ? -1 : 0);
                    int dirY = _DestinationSpatial.y > _CurrentSpatial.y ? 1 : (_DestinationSpatial.y < _CurrentSpatial.y ? -1 : 0);
                    int dirZ = _DestinationSpatial.z > _CurrentSpatial.z ? 1 : (_DestinationSpatial.z < _CurrentSpatial.z ? -1 : 0);

                    if (dirX == 0 && dirY == 0 && dirZ == 0)
                        break;

                    _CurrentSpatial.x += dirX;
                    _CurrentSpatial.y += dirY;
                    _CurrentSpatial.z += dirZ;

                    float fuelPerGridspace = ArtronPerStep / Mathf.Max(GridspacesPerStep, 1);
                    if (fluidLink.FuelLeft < fuelPerGridspace)
                    {
                        Debug.LogWarning("Engine_Navcom: Not enough Artron Energy to continue vortex movement!");
                        // TODO: Add crash/randomization logic here for vortex crash landings
                        // Example: Randomize one axis of _CurrentSpatial
                        return;
                    }
                    fluidLink.FuelLeft -= fuelPerGridspace;
                    _spatialDistanceAccumulator += 1f;
                    _spatialTimer -= timePerGridspace;
                }
            }
            // --- Pocket Movement (unchanged for vortex, can be customized) ---
            if (!HasReachedPocketDestination)
            {
                _pocketTimer += Time.deltaTime;
                if (_pocketTimer >= pocketStepDelay)
                {
                    int dirPX = _DestinationPocket.x > _CurrentPocket.x ? 1 : (_DestinationPocket.x < _CurrentPocket.x ? -1 : 0);
                    int dirPY = _DestinationPocket.y > _CurrentPocket.y ? 1 : (_DestinationPocket.y < _CurrentPocket.y ? -1 : 0);
                    int dirPZ = _DestinationPocket.z > _CurrentPocket.z ? 1 : (_DestinationPocket.z < _CurrentPocket.z ? -1 : 0);

                    _CurrentPocket.x += dirPX;
                    _CurrentPocket.y += dirPY;
                    _CurrentPocket.z += dirPZ;

                    if (dirPX != 0) _CurrentPocket.x = Mathf.Clamp(_CurrentPocket.x, Mathf.Min(_DestinationPocket.x, _CurrentPocket.x - dirPX), Mathf.Max(_DestinationPocket.x, _CurrentPocket.x - dirPX));
                    if (dirPY != 0) _CurrentPocket.y = Mathf.Clamp(_CurrentPocket.y, Mathf.Min(_DestinationPocket.y, _CurrentPocket.y - dirPY), Mathf.Max(_DestinationPocket.y, _CurrentPocket.y - dirPY));
                    if (dirPZ != 0) _CurrentPocket.z = Mathf.Clamp(_CurrentPocket.z, Mathf.Min(_DestinationPocket.z, _CurrentPocket.z - dirPZ), Mathf.Max(_DestinationPocket.z, _CurrentPocket.z - dirPZ));

                    if (dirPX != 0 && _CurrentPocket.x == _DestinationPocket.x) _CurrentPocket.x = _DestinationPocket.x;
                    if (dirPY != 0 && _CurrentPocket.y == _DestinationPocket.y) _CurrentPocket.y = _DestinationPocket.y;
                    if (dirPZ != 0 && _CurrentPocket.z == _DestinationPocket.z) _CurrentPocket.z = _DestinationPocket.z;

                    _pocketTimer = 0f;
                }
            }

            if (HasReachedSpatialDestination && HasReachedPocketDestination)
            {
                Debug.Log("NavCom: Vortex destination reached! Notifying TARDISMain for rematerialization.");
                tardisMain.NotifyDestinationReached();
            }
        }

        public void FlyTowardsDestinationFastReturn(float throttleSpeedNormalized, float speedMultiplier, float fuelMultiplier)
        {
            if (!isFunctional || !_isCircuitActive) return;
            if (throttleSpeedNormalized <= 0) return;

            var fluidLink = engineManager != null ? engineManager.fluidlinks : null;
            if (fluidLink == null)
            {
                Debug.LogWarning("Engine_Navcom: FluidLink not found, cannot consume Artron Energy.");
                return;
            }

            if (!HasReachedSpatialDestination)
            {
                float timePerGridspace = ((StepTime / (float)GridspacesPerStep) / Mathf.Max(throttleSpeedNormalized, 0.01f)) / Mathf.Max(speedMultiplier, 0.01f);
                _spatialTimer += Time.deltaTime;
                while (_spatialTimer >= timePerGridspace && !HasReachedSpatialDestination)
                {
                    int dirX = _DestinationSpatial.x > _CurrentSpatial.x ? 1 : (_DestinationSpatial.x < _CurrentSpatial.x ? -1 : 0);
                    int dirY = _DestinationSpatial.y > _CurrentSpatial.y ? 1 : (_DestinationSpatial.y < _CurrentSpatial.y ? -1 : 0);
                    int dirZ = _DestinationSpatial.z > _CurrentSpatial.z ? 1 : (_DestinationSpatial.z < _CurrentSpatial.z ? -1 : 0);

                    if (dirX == 0 && dirY == 0 && dirZ == 0)
                        break;

                    _CurrentSpatial.x += dirX;
                    _CurrentSpatial.y += dirY;
                    _CurrentSpatial.z += dirZ;

                    float fuelPerGridspace = (ArtronPerStep / Mathf.Max(GridspacesPerStep, 1)) * fuelMultiplier;
                    if (fluidLink.FuelLeft < fuelPerGridspace)
                    {
                        Debug.LogWarning("Engine_Navcom: Not enough Artron Energy to continue fast return movement!");
                        return;
                    }
                    fluidLink.FuelLeft -= fuelPerGridspace;
                    _spatialDistanceAccumulator += 1f;
                    _spatialTimer -= timePerGridspace;
                }
            }
            // Pocket movement (same as normal)
            if (!HasReachedPocketDestination)
            {
                _pocketTimer += Time.deltaTime;
                if (_pocketTimer >= pocketStepDelay)
                {
                    int dirPX = _DestinationPocket.x > _CurrentPocket.x ? 1 : (_DestinationPocket.x < _CurrentPocket.x ? -1 : 0);
                    int dirPY = _DestinationPocket.y > _CurrentPocket.y ? 1 : (_DestinationPocket.y < _CurrentPocket.y ? -1 : 0);
                    int dirPZ = _DestinationPocket.z > _CurrentPocket.z ? 1 : (_DestinationPocket.z < _CurrentPocket.z ? -1 : 0);

                    _CurrentPocket.x += dirPX;
                    _CurrentPocket.y += dirPY;
                    _CurrentPocket.z += dirPZ;

                    if (dirPX != 0) _CurrentPocket.x = Mathf.Clamp(_CurrentPocket.x, Mathf.Min(_DestinationPocket.x, _CurrentPocket.x - dirPX), Mathf.Max(_DestinationPocket.x, _CurrentPocket.x - dirPX));
                    if (dirPY != 0) _CurrentPocket.y = Mathf.Clamp(_CurrentPocket.y, Mathf.Min(_DestinationPocket.y, _CurrentPocket.y - dirPY), Mathf.Max(_DestinationPocket.y, _CurrentPocket.y - dirPY));
                    if (dirPZ != 0) _CurrentPocket.z = Mathf.Clamp(_CurrentPocket.z, Mathf.Min(_DestinationPocket.z, _CurrentPocket.z - dirPZ), Mathf.Max(_DestinationPocket.z, _CurrentPocket.z - dirPZ));

                    if (dirPX != 0 && _CurrentPocket.x == _DestinationPocket.x) _CurrentPocket.x = _DestinationPocket.x;
                    if (dirPY != 0 && _CurrentPocket.y == _DestinationPocket.y) _CurrentPocket.y = _DestinationPocket.y;
                    if (dirPZ != 0 && _CurrentPocket.z == _DestinationPocket.z) _CurrentPocket.z = _DestinationPocket.z;

                    _pocketTimer = 0f;
                }
            }

            if (HasReachedSpatialDestination && HasReachedPocketDestination)
            {
                Debug.Log("NavCom: Fast Return destination reached! Notifying TARDISMain for rematerialization.");
                tardisMain.NotifyDestinationReached();
            }
        }
    }
}