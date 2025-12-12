using System.Collections;
using TARDIS.Main;
using Unity.Mathematics;
using UnityEngine;

namespace TARDIS.Core
{
    public class NaviCore : MonoBehaviour
    {
        [SerializeField] _42Main mainscript;
        public CoreManager coreManager;
        public UtilityCore utilityCore;
        private Coroutine travelCoroutine;
        private bool isMoving = false;

        private void Start()
        {
            utilityCore = coreManager.utilityCore;
        }

        public int4 targetCoords; // gets updated by delta circuit
        public int4 currentCoords; // changes in this script - used for display and the int4 that moves from lastcoords to targetcoords
        public int4 startingCoords; // this is for lerp start
        public int4 lastCoords; // this is both for fast return

        public void UpdateTargetCoords(int4 target)
        {
            targetCoords = target;
        }

        public void BeginFlightNavigation()
        {
            // Logic to start navigating from startingCoords to targetCoords
            // Only start one travel coroutine
            if (travelCoroutine == null)
            {
                startingCoords = currentCoords;
                lastCoords = currentCoords;
                travelCoroutine = StartCoroutine(TravelToTarget());
            }
        }

        private enum TempSpeedModeEnum { Drift = 1, Normal = 10, Medium = 15, Maximum = 25 }

        private IEnumerator TravelToTarget()
        {
            // Keeps running: moves toward target when different, otherwise hovers consuming small fuel
            float speedDelay = 1f / (float)TempSpeedModeEnum.Normal;
            float hoverFuelPerSecond = 0.05f; // small fuel drain while hovering

            try
            {
                while (mainscript.currentFlightState == _42Main.ShipFlightState.InFlight)
                {
                    // If target is different, move one step toward it
                    if (!math.all(currentCoords == targetCoords))
                    {
                        if (!isMoving)
                        {
                            // mark movement start
                            isMoving = true;
                        }

                        int4 direction = new int4(
                            targetCoords.x > currentCoords.x ? 1 : (targetCoords.x < currentCoords.x ? -1 : 0),
                            targetCoords.y > currentCoords.y ? 1 : (targetCoords.y < currentCoords.y ? -1 : 0),
                            targetCoords.z > currentCoords.z ? 1 : (targetCoords.z < currentCoords.z ? -1 : 0),
                            targetCoords.w > currentCoords.w ? 1 : (targetCoords.w < currentCoords.w ? -1 : 0)
                        );

                        currentCoords += direction;

                        float remainingDistance = math.distance((float4)currentCoords, (float4)targetCoords);
                        Debug.Log($"Traveling: CurrentCoords = {currentCoords}, RemainingDistance = {remainingDistance}");

                        float fuelConsumption = GetFuelConsumptionRate(TempSpeedModeEnum.Normal) * math.distance((float4)direction, float4.zero);
                        if (utilityCore != null)
                            utilityCore.ConsumeFuel(fuelConsumption);

                        if (utilityCore != null && utilityCore.currentFuel <= 0)
                        {
                            Debug.LogWarning("Out of fuel! Navigation halted.");
                            yield break;
                        }

                        // wait based on speed
                        yield return new WaitForSeconds(speedDelay);
                    }
                    else
                    {
                        // Target reached: enter hover state (small fuel drain per second) but keep checking for target changes
                        if (isMoving)
                        {
                            isMoving = false;
                            Debug.Log("Navigation complete. Target reached. Entering hover state.");
                        }

                        if (utilityCore != null)
                            utilityCore.ConsumeFuel(hoverFuelPerSecond);

                        if (utilityCore != null && utilityCore.currentFuel <= 0)
                        {
                            Debug.LogWarning("Out of fuel while hovering! Navigation halted.");
                            yield break;
                        }

                        // check again next second
                        yield return new WaitForSeconds(1f);
                    }
                }
            }
            finally
            {
                // clear the running coroutine reference
                travelCoroutine = null;
            }
        }

        private float GetFuelConsumptionRate(TempSpeedModeEnum speedMode)
        {
            switch (speedMode)
            {
                case TempSpeedModeEnum.Drift: return 0.1f; // 1 fuel per 10 distance units
                case TempSpeedModeEnum.Normal: return 0.2f; // 1 fuel per 5 distance units
                case TempSpeedModeEnum.Medium: return 0.33f; // 1 fuel per 3 distance units
                case TempSpeedModeEnum.Maximum: return 1f; // 1 fuel per 1 distance unit
                default: return 1f;
            }
        }
    }
}