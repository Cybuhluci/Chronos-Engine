using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System

// Define an enum for clarity and type safety for interaction types
public enum EInteractionType
{
    Normal,         // Default crosshair, no interaction possible
    InteractShort,  // Basic interaction (e.g., pressing a button, opening a door)
    Prop,           // Pickupable/holdable object (e.g., an item)
    LongInteract    // For actions requiring a longer press or holding (e.g., charging, specific TARDIS controls)
}

public interface IInteractable
{
    // Method to be called when the player interacts with the object
    void Interact();
    // Method to get the type of interaction for crosshair feedback
    EInteractionType GetInteractionType();
}

public class PlayerLookInteract : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Camera playerCamera; // Drag your main camera here
    [SerializeField] private PlayerInput playerInput; // Drag your PlayerInput component here

    [Header("Raycast Settings")]
    [SerializeField] private float raycastDistance = 5f;
    [SerializeField] private LayerMask interactableLayer; // Define which layers are interactable

    [Header("Crosshair GameObjects")]
    [SerializeField] private GameObject crosshairDefault;
    [SerializeField] private GameObject crosshairInteractable;
    [SerializeField] private GameObject crosshairProp;
    [SerializeField] private GameObject crosshairLongInteract; // Corrected typo here

    // Cached Input Actions for performance (gets references once instead of string lookup every frame)
    private InputAction _mouseLookAction;
    private InputAction _interactAction;

    void Awake() // Changed Start to Awake for input action caching
    {
        // Get component references if not assigned in Inspector
        if (playerCamera == null) playerCamera = Camera.main; // Fallback to main camera
        if (playerInput == null) playerInput = GetComponent<PlayerInput>(); // Fallback to get component on same GameObject

        // Cache the input actions
        if (playerInput != null && playerInput.actions != null)
        {
            _mouseLookAction = playerInput.actions.FindAction("Look"); // Assuming "Look" action for mouse delta/look
            _interactAction = playerInput.actions.FindAction("Interact");
        }

        // Lock cursor on Awake for first-person control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; // Typically want cursor invisible when locked

        ResetCrosshairs(); // Initialize crosshairs
    }

    void Update()
    {
        // For first-person interaction, the ray should originate from the camera's center and go forward.
        // Using playerCamera.transform.forward is much more robust for a fixed crosshair than mouse position.
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // Check if the ray hits an object on the interactable layer
        if (Physics.Raycast(ray, out hit, raycastDistance, interactableLayer))
        {
            // Try to get the IInteractable component from the hit object
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                // Set the crosshair based on the interactable type
                SetCrosshair(interactable.GetInteractionType());

                // If the interact button was pressed this frame, interact with the object
                if (_interactAction != null && _interactAction.WasPressedThisFrame())
                {
                    interactable.Interact();
                }
            }
            else
            {
                // Hit something on the interactableLayer, but it's not IInteractable
                SetCrosshair(EInteractionType.Normal);
            }
        }
        else
        {
            // No interactable object detected within raycast distance
            SetCrosshair(EInteractionType.Normal);
        }
    }

    // Activates the correct crosshair GameObject based on interaction type
    void SetCrosshair(EInteractionType interactionType)
    {
        ResetCrosshairs(); // Turn off all crosshairs first

        switch (interactionType)
        {
            case EInteractionType.Normal:
                if (crosshairDefault != null) crosshairDefault.SetActive(true);
                break;
            case EInteractionType.InteractShort:
                if (crosshairInteractable != null) crosshairInteractable.SetActive(true);
                break;
            case EInteractionType.Prop:
                if (crosshairProp != null) crosshairProp.SetActive(true);
                break;
            case EInteractionType.LongInteract: // Added case for LongInteract crosshair
                if (crosshairLongInteract != null) crosshairLongInteract.SetActive(true);
                break;
            default: // Fallback in case of an unhandled type (shouldn't happen with enum)
                if (crosshairDefault != null) crosshairDefault.SetActive(true);
                break;
        }
    }

    // Deactivates all crosshair GameObjects
    void ResetCrosshairs()
    {
        // Add null checks to prevent errors if GameObjects are not assigned
        if (crosshairDefault != null) crosshairDefault.SetActive(false);
        if (crosshairInteractable != null) crosshairInteractable.SetActive(false);
        if (crosshairProp != null) crosshairProp.SetActive(false);
        if (crosshairLongInteract != null) crosshairLongInteract.SetActive(false); // Also reset this one
    }

    // You might want methods to enable/disable interaction, similar to your FPC controls
    public void SetInteractionEnabled(bool enabled)
    {
        if (enabled)
        {
            // Potentially re-enable input actions if they were disabled
            _interactAction?.Enable();
            SetCrosshair(EInteractionType.Normal); // Show default crosshair when interaction is enabled
        }
        else
        {
            _interactAction?.Disable();
            ResetCrosshairs(); // Hide all crosshairs when interaction is disabled
        }
    }
}