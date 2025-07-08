using UnityEngine;

public class TARDISConsoleManager : MonoBehaviour
{
    [Header("Dematerialisation Circuit Subsystems")]
    public Console_Handbrake timeRotorHandbrake; // time rotor handbrake
    public Console_Throttle spaceTimeThrottle; // space time throttle

    [Header("Navigational Computer Subsystems")]
    public Console_Telepathic telepathicCircuit;

    [Header("Fluid Link Subsystems")]
    public GameObject refueller;

    [Header("Interstitial Antennae Subsystems")]
    public GameObject communicator;

    [Header("Chameleon Circuit Subsytems")]
    public GameObject chameleonCircuit;

    [Header("Shield Generator Subsystems")]
    public GameObject exteriorBulkheadLock; // outside door lock (not needed in current version of game, no door even exists yet)

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