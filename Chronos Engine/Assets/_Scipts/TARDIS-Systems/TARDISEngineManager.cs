using UnityEngine;

public class TARDISEngineManager : MonoBehaviour
{
    // --- im going to put each engine's systems full legal name as a tooltip maybe.
    [Header("Essential Engine Subsystems")]
    public Engine_Demat dematCircuit;
    public Engine_FluidLink fluidlinks;

    [Header("Optional Engine Subsytems")] 
    public Engine_Chameleon chameleon;
    public Engine_Antennae antennae;
    public Engine_TemporalGrace temporalgrace;
    public Engine_ShieldGen shieldgenerator;
    public Engine_Navcom navigationcom;
    public Engine_Stabilisers stabilisers;

    void Awake()
    {
        // Get references if not assigned in Inspector
        if (navigationcom == null) navigationcom = GetComponentInChildren<Engine_Navcom>();
        if (dematCircuit == null) dematCircuit = GetComponentInChildren<Engine_Demat>();
    }
}