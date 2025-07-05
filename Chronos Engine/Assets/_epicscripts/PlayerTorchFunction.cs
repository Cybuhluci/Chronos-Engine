using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTorchFunction : MonoBehaviour
{
    [Header("Torch Settings")]
    public Light torchLight; // Assign the flashlight (Spotlight) in the inspector

    [Header("Battery Settings")]
    public float maxBattery = 100f; // Max battery life
    public float batteryLife; // Current battery level
    public float drainRate = 10f; // Battery drain per second when ON
    public float rechargeRate = 5f; // Battery recharge per second when OFF

    public bool isOn = false; // Flashlight state
    public PlayerInput playerInput; // Reference to Player Input System

    void Awake()
    {
        playerInput = GetComponentInChildren<PlayerInput>(); // Get PlayerInput component
        batteryLife = maxBattery; // Set battery to full at start
        torchLight.enabled = false;
        isOn = false;
    }

    void OnEnable()
    {
        playerInput.actions["torch"].performed += ToggleTorch; // Bind torch action
    }

    void OnDisable()
    {
        playerInput.actions["torch"].performed -= ToggleTorch; // Unbind torch action
    }

    void Update()
    {
        // Drain battery when ON
        if (isOn && batteryLife > 0)
        {
            batteryLife -= drainRate * Time.deltaTime;
            if (batteryLife <= 0)
            {
                batteryLife = 0;
                isOn = false; // Auto-turn off if battery runs out
                UpdateTorchState();
            }
        }
        // Recharge battery when OFF
        else if (!isOn && batteryLife < maxBattery)
        {
            batteryLife += rechargeRate * Time.deltaTime;
            if (batteryLife > maxBattery)
            {
                batteryLife = maxBattery;
            }
        }
    }

    void ToggleTorch(InputAction.CallbackContext context)
    {
        if (batteryLife > 0)
        {
            isOn = !isOn;
            UpdateTorchState();
        }
    }

    void UpdateTorchState()
    {
        if (torchLight != null)
        {
            torchLight.enabled = isOn;
        }
    }
}
