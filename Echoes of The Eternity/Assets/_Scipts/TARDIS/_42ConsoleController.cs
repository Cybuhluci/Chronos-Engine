using TARDIS.Main;
using UnityEngine;

public abstract class _42ConsoleController : MonoBehaviour
{
    [Header("Dependencies")]
    [HideInInspector] protected _42Main tardisMain;

    public Transform markerAnchor; // Anchor for the marker, used for positioning in the console

    private void Awake()
    {
        tardisMain ??= FindAnyObjectByType<_42Main>();
    }
    private void Start()
    {
        tardisMain ??= FindAnyObjectByType<_42Main>();
    }

    public virtual string GetButtonState() { return _isCircuitActive ? "On" : "Off"; }
    public virtual string GetButtonFunctionality() { return _isFunctional ? "Working" : "Broken"; }

    [Header("Subsystem Common Settings")]
    [SerializeField] protected bool _isCircuitActive = false; // Is the circuit actively "doing its stuff"?
    public bool IsCircuitActive => _isCircuitActive;

    // isFunctional implies structural integrity (e.g., not broken, not fried).
    // If false, the circuit cannot be activated or perform its function.
    [SerializeField] protected bool _isFunctional = true;
    public bool IsFunctional => _isFunctional;

    // flight event button lockoff management
    [SerializeField] protected bool _inFlightEvent = false; // If true, the button cannot be pressed during flight events.
    public bool InFlightEvent => _inFlightEvent;

    public virtual void NotifyDematCircuitButtonPressed()
    {
        Debug.Log($"Notifying DematerialisationCircuit: Button {this.GetType().Name} pressed.");
        //FlightCore.NotifyButtonPressed(this.GetType().Name);
    }

    protected abstract void OnCircuitActivated();
    protected abstract void OnCircuitDeactivated();

    public virtual void ActivateCircuit()
    {
        if (_inFlightEvent) // Check if the circuit is in a flight event
        {
            Debug.Log($"{gameObject.name}: Cannot activate circuit. In flight event.");
            NotifyDematCircuitButtonPressed();
            return;
        }

        if (!_isFunctional)
        {
            Debug.Log($"{gameObject.name}: Cannot activate circuit. Not functional.");
            return;
        }
        if (!_isCircuitActive)
        {
            _isCircuitActive = true; // Set the active state to true
            OnCircuitActivated(); // Call the specific implementation for "on"
            //Debug.Log($"{gameObject.name}: Circuit Activated.");
        }
    }

    public virtual void DeactivateCircuit()
    {
        if (_inFlightEvent) // Check if the circuit is in a flight event
        {
            Debug.Log($"{gameObject.name}: Cannot activate circuit. In flight event.");
            NotifyDematCircuitButtonPressed();
            return;
        }

        if (!_isFunctional)
        {
            Debug.Log($"{gameObject.name}: Cannot deactivate circuit. Not functional.");
            return;
        }
        if (_isCircuitActive)
        {
            _isCircuitActive = false; // Set the active state to false
            OnCircuitDeactivated(); // Call the specific implementation for "off"
            //Debug.Log($"{gameObject.name}: Circuit Deactivated.");
        }
    }

    public virtual void ToggleCircuit()
    {
        if (_inFlightEvent) // Check if the circuit is in a flight event
        {
            Debug.Log($"{gameObject.name}: Cannot activate circuit. In flight event.");
            NotifyDematCircuitButtonPressed();
            return;
        }

        if (_isCircuitActive)
        {
            DeactivateCircuit(); // Call the specific implementation for "on"
            //Debug.Log($"{gameObject.name}: Circuit Toggled ON.");
        }
        else
        {
            ActivateCircuit(); // Call the specific implementation for "off"
            //Debug.Log($"{gameObject.name}: Circuit Toggled OFF.");
        }
    }

    public virtual void SetInFlightEvent(bool state)
    {
        _inFlightEvent = state; // Flip the active state
    }

}
