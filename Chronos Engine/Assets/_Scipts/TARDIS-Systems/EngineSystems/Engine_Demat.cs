using UnityEngine;
using System.Collections; // Required for Coroutines

public class Engine_Demat : TARDISSubsystemController
{
    [Header("Dependencies")]
    [SerializeField] private TARDISMain tardisMain;
    [SerializeField] private TARDISConsoleManager consoleManager; // Now a direct dependency
    [SerializeField] private TARDISEngineManager engineManager;

    [Header("Flight Parameters")]
    public float dematerializationDuration = 5f; // How long demat takes
    public float materializationDuration = 5f; // How long mat takes

    [Header("Audio")]
    public AudioSource tardisSoundSource; // For demat/mat/flight sounds
    public AudioClip dematSound; // Assign in Inspector
    public AudioClip matSound;    // Assign in Inspector
    public AudioClip flightLoopSound; // Assign in Inspector (should loop)
    public AudioClip destinationReachedSound; // Sound for when destination is reached

    // Internal coroutine reference to manage ongoing sequences
    private Coroutine _currentDematSequence;

    private void Awake()
    {
        ToggleCircuit();

        // Ensure we get references for our dependencies
        if (tardisMain == null) tardisMain = FindAnyObjectByType<TARDISMain>();
        // Get engineManager from parent or by type if needed, as before
        if (engineManager == null) engineManager = GetComponentInParent<TARDISEngineManager>();
        if (tardisSoundSource == null) tardisSoundSource = GetComponent<AudioSource>();

        // NEW: Get the consoleManager reference from TARDISMain
        // This is crucial for Engine_Demat to act as a "brain"
        if (tardisMain != null && consoleManager == null)
        {
            consoleManager = tardisMain.consoleManager;
        }

        // Error checks
        if (tardisMain == null) Debug.LogError("Engine_Demat: TARDISMain reference is missing!");
        if (engineManager == null) Debug.LogError("Engine_Demat: TARDISEngineManager reference is missing!");
        if (consoleManager == null) Debug.LogError("Engine_Demat: TARDISConsoleManager reference is missing! Cannot check console controls.");
        if (tardisSoundSource == null) Debug.LogWarning("Engine_Demat: AudioSource missing. Flight sounds won't play.");
    }

    // --- TARDISSubsystemController Implementations (Keep as is for now) ---
    protected override void OnCircuitActivated() { Debug.Log("Engine_Demat: Circuit ENGAGED"); }
    protected override void OnCircuitDeactivated()
    {
        Debug.Log("Engine_Demat: Circuit RELEASED");
        if (_currentDematSequence != null)
        {
            StopCoroutine(_currentDematSequence);
            _currentDematSequence = null;
            if (tardisMain != null) tardisMain.SetTARDISState(TARDISMain.TARDISState.Landed); // Force land
            if (tardisSoundSource != null) tardisSoundSource.Stop(); // Stop any sound
            Debug.Log("Engine_Demat: Aborting flight sequence due to circuit deactivation!");
        }
    }
    public override string GetCircuitStatus() { return _isCircuitActive ? "Engaged" : "Released"; }

    // --- NEW: Public method for TARDISMain to call to attempt initiating flight ---
    public void AttemptFlight()
    {
        if (!isFunctional || !_isCircuitActive)
        {
            Debug.Log("Engine_Demat: Dematerialisation Circuit not functional or active. Cannot attempt flight.");
            return;
        }

        // --- NEW: Perform all pre-flight checks directly here ---
        if (consoleManager == null || engineManager == null ||
            consoleManager.timeRotorHandbrake == null || engineManager.navigationcom == null ||
            consoleManager.spaceTimeThrottle == null || consoleManager.telepathicCircuit == null)
        {
            Debug.LogError("Engine_Demat: Missing essential console/engine components for flight. Cannot launch.");
            return;
        }

        bool handbrakeOff = !consoleManager.timeRotorHandbrake.IsEngaged;
        bool throttleActiveAndPulled = consoleManager.spaceTimeThrottle.IsCircuitActive && consoleManager.spaceTimeThrottle.currentThrottleValue > 0;

        // Ensure ALL necessary circuits (console and engine) are operational
        bool allCircuitsReady = consoleManager.telepathicCircuit.IsFullyOperational() &&
                                engineManager.navigationcom.IsFullyOperational() &&
                                consoleManager.spaceTimeThrottle.IsFullyOperational();
        // Add other engine/console systems here as they become relevant
        // e.g., && engineManager.fluidLinks.IsFullyOperational()
        //       && consoleManager.stabilisers.IsFullyOperational() etc.


        Vector3Int targetSpatial = consoleManager.telepathicCircuit.GetDestination();
        Vector3Int targetPocket = consoleManager.telepathicCircuit.GetPocketDestination();
        bool destinationSet = (targetSpatial != Vector3Int.zero || targetPocket != Vector3Int.zero);

        if (handbrakeOff && throttleActiveAndPulled && allCircuitsReady && destinationSet)
        {
            Debug.Log("Engine_Demat: All conditions met! Initiating dematerialization sequence...");

            // If previous sequence is running, stop it (safety)
            if (_currentDematSequence != null) StopCoroutine(_currentDematSequence);

            // Start the dematerialization sequence with the confirmed targets
            _currentDematSequence = StartCoroutine(DematerializationSequence(targetSpatial, targetPocket));
        }
        //else
        //{
        //    // Provide specific reasons why flight cannot be initiated
        //    string reason = "Flight conditions not met: ";
        //    if (!handbrakeOff) reason += "Handbrake engaged. ";
        //    if (!throttleActiveAndPulled) reason += "Throttle not active or pulled. ";
        //    if (!allCircuitsReady) reason += "Not all required circuits are ready. ";
        //    if (!destinationSet) reason += "Destination not set. ";
        //    Debug.LogWarning("Engine_Demat: " + reason);
        //}
    }

    // Materialization is now triggered via TARDISMain's Rematerialize()
    // and this method itself is now private and part of the sequence.
    private IEnumerator MaterializationSequence() // Changed to private
    {
        tardisMain.SetTARDISState(TARDISMain.TARDISState.Materializing);
        tardisMain.NotifyMaterializationStarted();

        Debug.Log("TARDIS: Materializing...");

        if (tardisSoundSource != null && tardisSoundSource.clip == flightLoopSound)
        {
            tardisSoundSource.Stop();
            tardisSoundSource.loop = false;
        }

        if (tardisSoundSource != null && matSound != null)
        {
            tardisSoundSource.PlayOneShot(matSound);
        }

        yield return new WaitForSeconds(materializationDuration);

        tardisMain.SetTARDISState(TARDISMain.TARDISState.Landed);
        tardisMain.UpdateCurrentLocationsFromNavcom();
        tardisMain.NotifyFlightEnded();

        Debug.Log($"TARDIS: Successfully materialized at Spatial: {tardisMain.currentSpatialLocation}, Chronal: {tardisMain.currentPocketLocation}");

        _currentDematSequence = null;
    }

    // --- Coroutines for TARDIS Sequences ---
    // This coroutine still takes parameters, but these parameters will now
    // come directly from AttemptFlight() after it has validated them.
    private IEnumerator DematerializationSequence(Vector3Int targetSpatial, Vector3Int targetPocket)
    {
        tardisMain.SetTARDISState(TARDISMain.TARDISState.Dematerializing);
        tardisMain.NotifyDematerializationStarted();

        Debug.Log("TARDIS: Dematerializing...");

        if (tardisSoundSource != null && dematSound != null)
        {
            tardisSoundSource.PlayOneShot(dematSound);
        }

        yield return new WaitForSeconds(dematerializationDuration);

        // Update NavCom with the TARDIS's *actual* current location (where it dematerialized from)
        // and set the *destination* for the flight.
        if (engineManager != null && engineManager.navigationcom != null)
        {
            engineManager.navigationcom.SetCurrentLocation(tardisMain.currentSpatialLocation, tardisMain.currentPocketLocation);
            engineManager.navigationcom.SetDestination(targetSpatial, targetPocket);
        }

        tardisMain.SetTARDISState(TARDISMain.TARDISState.Flying);
        tardisMain.NotifyFlightStarted();

        Debug.Log($"TARDIS: Now in Vortex Flight towards Spatial: {targetSpatial}, Chronal: {targetPocket}");

        if (tardisSoundSource != null && flightLoopSound != null)
        {
            tardisSoundSource.clip = flightLoopSound;
            tardisSoundSource.loop = true;
            tardisSoundSource.Play();
        }
    }

    // Public method to start materialization, called by TARDISMain
    public void StartMaterializationSequenceFromMain()
    {
        if (!isFunctional || !_isCircuitActive)
        {
            Debug.Log("Engine_Demat: Dematerialisation Circuit not functional or active. Cannot start materialization.");
            return;
        }

        if (_currentDematSequence != null) StopCoroutine(_currentDematSequence);
        _currentDematSequence = StartCoroutine(MaterializationSequence());
    }
}