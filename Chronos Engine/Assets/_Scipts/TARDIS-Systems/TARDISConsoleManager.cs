using UnityEngine;
using Luci.TARDIS;
using Luci.TARDIS.Engine;

namespace Luci.TARDIS.Console
{
    public class TARDISConsoleManager : MonoBehaviour
    {
        [Header("Dematerialisation Circuit Subsystems")]
        public Console_Handbrake timeRotorHandbrake; // time rotor handbrake
        public Console_Throttle spaceTimeThrottle; // space time throttle
        public Console_FastReturn fastReturn; // fast return switch

        [Header("Navigational Computer Subsystems")]
        public Console_Telepathic telepathicCircuit;
        public Console_VortexFlight vortexFlight; // vortex flight switch
        public Console_DeltaCircuit deltaCircuit; // coordinate control switch
        public Console_ExteriorFacing exteriorFacing; // exterior facing switch

        [Header("Fluid Link Subsystems")]
        public Console_Refueller refueller;

        [Header("Interstitial Antennae Subsystems")]
        public GameObject communicator;

        [Header("Chameleon Circuit Subsytems")]
        public GameObject chameleonCircuit;

        [Header("Shield Generator Subsystems")]
        public Console_PhysicalLock physicalLock;

        [Header("Stabiliser Subsystems")]
        public GameObject stabilisers;

        [Header("Temporal Grace Unit Subsystems")]
        public GameObject temporalGraceUnit;

        void Awake()
        {
            // Get references if not assigned in Inspector
            if (telepathicCircuit == null) telepathicCircuit = GetComponentInChildren<Console_Telepathic>();
            if (timeRotorHandbrake == null) timeRotorHandbrake = GetComponentInChildren<Console_Handbrake>();
            if (spaceTimeThrottle == null) spaceTimeThrottle = GetComponentInChildren<Console_Throttle>();

            if (spaceTimeThrottle != null) spaceTimeThrottle.SetThrottle(0); // Ensures throttle is at zero
        }
    }
}