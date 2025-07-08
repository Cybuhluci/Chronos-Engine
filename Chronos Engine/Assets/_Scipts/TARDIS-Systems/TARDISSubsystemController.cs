using UnityEngine;

// Define common functionality for all TARDIS subsystems
public abstract class TARDISSubsystemController : MonoBehaviour
{
    [Header("Subsystem Common Settings")]
    [SerializeField] protected bool _isCircuitActive = false; // Is the circuit actively "doing its stuff"?

    // isFunctional implies structural integrity (e.g., not broken, not fried).
    // If false, the circuit cannot be activated or perform its function.
    public bool isFunctional = true;

    // Public properties to expose states (read-only for external access)
    public bool IsCircuitActive => _isCircuitActive;

    // --- Abstract methods that concrete subsystems MUST implement ---
    // These methods define the specific "on" and "off" actions for each circuit's functionality.
    // ToggleCircuit will call these based on _isCircuitActive state.
    protected abstract void OnCircuitActivated();
    protected abstract void OnCircuitDeactivated();

    // --- Methods to be overridden or used by concrete subsystems (TARDIS State Events) ---
    public virtual void OnTARDISDematerialize() { Debug.Log($"{gameObject.name}: TARDIS dematerializing."); }
    public virtual void OnTARDISMaterialize() { Debug.Log($"{gameObject.name}: TARDIS materializing."); }
    public virtual void OnTARDISFlightStart() { Debug.Log($"{gameObject.name}: TARDIS flight started."); }
    public virtual void OnTARDISFlightEnd() { Debug.Log($"{gameObject.name}: TARDIS flight ended."); }

    // --- Core control method from your design ---

    /// <summary>
    /// Toggles the active functional state of the circuit.
    /// This method will only change the circuit's active state if it is currently functional.
    /// Physical lever interaction should call this method.
    /// </summary>
    public virtual void ToggleCircuit()
    {
        if (!isFunctional) // Now only check if functional
        {
            Debug.Log($"{gameObject.name}: Cannot toggle circuit. Not functional.");
            return;
        }

        // If we reach here, it means the circuit IS functional, so we can toggle its active state.
        _isCircuitActive = !_isCircuitActive; // Flip the active state

        if (_isCircuitActive)
        {
            OnCircuitActivated(); // Call the specific implementation for "on"
            //Debug.Log($"{gameObject.name}: Circuit Toggled ON.");
        }
        else
        {
            OnCircuitDeactivated(); // Call the specific implementation for "off"
            //Debug.Log($"{gameObject.name}: Circuit Toggled OFF.");
        }
    }

    // Removed PowerCircuit() as it's no longer managed per-subsystem.

    /// <summary>
    /// Abstract method for concrete subclasses to return their specific status.
    /// </summary>
    public abstract string GetCircuitStatus();

    // --- Common utility methods ---

    // Checks if the circuit is currently functional AND actively doing its stuff
    public bool IsFullyOperational()
    {
        return isFunctional && _isCircuitActive;
    }
}