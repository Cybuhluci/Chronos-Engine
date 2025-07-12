using UnityEngine;
using Luci.TARDIS;
using Luci.TARDIS.Engine;

namespace Luci.TARDIS.Console
{
    public class Console_Refueller : TARDISSubsystemController
    {
        [Header("Dependencies")]
        [SerializeField] private TARDISMain tardisMain;
        [SerializeField] private TARDISConsoleManager consoleManager;
        [SerializeField] private TARDISEngineManager engineManager;
        public bool IsEngaged => _isCircuitActive;
        public float RefuelRate = 1.0f; // Time between refuels in seconds
        public float RefuelAmount = 0.1f; // Amount of fuel added per refuel action
        // the current fuel level is stored in the Fluid Links.

        private float _refuelTimer = 0f;

        // --- TARDISSubsystemController Implementations ---

        // This method is called by the base ToggleCircuit() when _isCircuitActive becomes TRUE.
        protected override void OnCircuitActivated()
        {
            Debug.Log("Console_Refueller: ENGAGED");
        }

        // This method is called by the base ToggleCircuit() when _isCircuitActive becomes FALSE.
        protected override void OnCircuitDeactivated()
        {
            Debug.Log("Console_Refueller: DISENGAGED");
        }

        // This method provides the current status string, using the base class's _isCircuitActive.
        public override string GetCircuitStatus()
        {
            return _isCircuitActive ? "Engaged" : "Released";
        }

        void Update()
        {
            if (_isCircuitActive && isFunctional && engineManager != null && engineManager.fluidlinks != null)
            {
                _refuelTimer += Time.deltaTime;
                if (_refuelTimer >= RefuelRate)
                {
                    engineManager.fluidlinks.FuelLeft = Mathf.Min(engineManager.fluidlinks.FuelLeft + RefuelAmount, engineManager.fluidlinks.maxFuelCapacity);
                    _refuelTimer = 0f;
                }
            }
            else
            {
                _refuelTimer = 0f;
            }
        }
    }
}
