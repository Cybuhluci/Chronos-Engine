using UnityEngine;

public class Console_Handbrake : TARDISSubsystemController
{
    [Header("Dependencies")]
    [SerializeField] private TARDISMain tardisMain;
    [SerializeField] private TARDISConsoleManager consoleManager;
    [SerializeField] private TARDISEngineManager engineManager;
    public bool IsEngaged => _isCircuitActive;

    private void Awake()
    {
        {
            ToggleCircuit(); // This will activate the circuit, making the handbrake engaged
        }

        Debug.Log($"Handbrake: Initial state is {GetCircuitStatus()}");
    }

    // --- TARDISSubsystemController Implementations ---

    // This method is called by the base ToggleCircuit() when _isCircuitActive becomes TRUE.
    // For the handbrake, 'Activated' means 'Engaged'.
    protected override void OnCircuitActivated()
    {
        Debug.Log("Console_Handbrake: ENGAGED");
    }

    // This method is called by the base ToggleCircuit() when _isCircuitActive becomes FALSE.
    // For the handbrake, 'Deactivated' means 'Released'.
    protected override void OnCircuitDeactivated()
    {
        Debug.Log("Console_Handbrake: RELEASED");
    }

    // This method provides the current status string, using the base class's _isCircuitActive.
    public override string GetCircuitStatus()
    {
        return _isCircuitActive ? "Engaged" : "Released";
    }
}