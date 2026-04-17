using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Luci.Interactions
{
    public enum InteractionType // actually just what text the type will be, doesnt actually change much
    {
        Interact,
        Open,
        Collect,
        Kill,
        Pick,
        Talk,
        Pickpocket,
        None
    }

    // Simple interactable contract. Any component that wants to be interactable can implement this.
    public interface IInteractable
    {
        // Called when player interacts (presses the interact key)
        void OnInteract(GameObject interactor);

        // Short prompt to display (e.g. "Open Door" / "Pick Lock")
        string GetInteractionPrompt();

        // Whether this object is a press or hold interaction
        InteractionType GetInteractionType();
    }

    public class PlayerInteractionScript : MonoBehaviour
        // this script is put on the camera, so using transform.forward will be the direction the player is looking at,
        // and transform.position will be the position of the camera.
    {
        // this bool can be added upon after basic look interactions are implemented and work
        [SerializeField] private bool lookInteractables = true; // if false, the player can interact with objects in a radius around them instead of looking at them

        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private LayerMask interactableLayer;

        [SerializeField] private Image interactionCircle; 
        [SerializeField] private TMP_Text interactionText; 
        [SerializeField] private TMP_Text interactionType; 

        [Header("Prompt Positioning")]
        [Tooltip("Offset from the interactable's transform position where the prompt will be spawned (world space).")]
        [SerializeField] private Vector3 promptOffset = new Vector3(0f, 0.25f, 0f);

        [SerializeField] private PlayerInput playerInput;

        private IInteractable currentInteractable;
        private float _holdTimer = 0f;
        private bool _holdInProgress = false;
        private bool _holdCompleted = false;
        // no longer using instantiated world prompts; UI text fields on HUD are used instead

        private void Update()
        {
            CheckForInteractable();
            HandleInteractionInput();
        }

        private void CheckForInteractable()
        {
            // Raycast forward from the camera to find an interactable object
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, interactionRange, interactableLayer))
            {
                var interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    // If the interactable exposes an "isActive" flag and it is false, don't target it
                    if (!IsInteractableActive(interactable))
                    {
                        // lost focus if we previously had one
                        if (currentInteractable != null)
                        {
                            currentInteractable = null;
                            ResetHoldState();
                            // clear HUD prompts
                            if (interactionText != null) interactionText.text = "";
                            if (interactionType != null) interactionType.text = "";
                            if (interactionCircle != null) interactionCircle.fillAmount = 0f;
                        }
                        return;
                    }

                    if (currentInteractable != interactable)
                    {
                        // New target: reset any hold state
                        ResetHoldState();
                        currentInteractable = interactable;

                        // Populate HUD texts (no instantiation). Use the provided TMP_Text fields on the HUD.
                        if (interactionText != null)
                        {
                            try { interactionText.text = currentInteractable.GetInteractionPrompt(); } catch { interactionText.text = ""; }
                        }
                        if (interactionType != null)
                        {
                            try { interactionType.text = currentInteractable.GetInteractionType().ToString(); } catch { interactionType.text = ""; }
                        }
                        if (interactionCircle != null) interactionCircle.fillAmount = 0f;
                    }
                    return;
                }
            }

            if (currentInteractable != null)
            {
                // lost focus
                currentInteractable = null;
                // TODO: clear UI prompt
                ResetHoldState();
                // clear HUD prompts
                if (interactionText != null) interactionText.text = "";
                if (interactionType != null) interactionType.text = "";
                if (interactionCircle != null) interactionCircle.fillAmount = 0f;
            }
        }

        private void HandleInteractionInput()
        {
            if (currentInteractable == null) return;

            // determine interaction type
            InteractionType type = InteractionType.Interact;
            try { type = currentInteractable.GetInteractionType(); } catch { type = InteractionType.Interact; }

            // Input System action if present
            var hasInputSystem = playerInput != null && playerInput.actions["Interact"] != null;

            if (type == InteractionType.Interact)
            {
                bool interactPressed = false;

                interactPressed = playerInput.actions["Interact"].WasPressedThisFrame();

                if (interactPressed)
                {
                    currentInteractable.OnInteract(gameObject);
                }
            }
            else // Hold
            {
                bool interactPressed = false;

                interactPressed = playerInput.actions["Interact"].WasPressedThisFrame();

                if (interactPressed)
                {
                    currentInteractable.OnInteract(gameObject);
                }
            }
        }

        private void ResetHoldState()
        {
            _holdInProgress = false;
            _holdTimer = 0f;
            _holdCompleted = false;
            if (interactionCircle != null) interactionCircle.fillAmount = 0f;
        }

        // Try to detect a common pattern for an "isActive" flag on the interactable MonoBehaviour.
        private bool IsInteractableActive(IInteractable interactable)
        {
            var mb = interactable as MonoBehaviour;
            if (mb == null) return true;

            // look for common field/property names
            var t = mb.GetType();
            var f = t.GetField("isactive");
            if (f != null && f.FieldType == typeof(bool))
            {
                return (bool)f.GetValue(mb);
            }

            // commented out, but keep just in case they somehow make it work.
            //f = t.GetField("isActive");
            //if (f != null && f.FieldType == typeof(bool))
            //{
            //    return (bool)f.GetValue(mb);
            //}

            //var p = t.GetProperty("isActive");
            //if (p != null && p.PropertyType == typeof(bool))
            //{
            //    return (bool)p.GetValue(mb);
            //}
            //p = t.GetProperty("IsActive");
            //if (p != null && p.PropertyType == typeof(bool))
            //{
            //    return (bool)p.GetValue(mb);
            //}

            // default: treat as inactive
            return false;
        }

        // Useful debug visualization in the editor
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, transform.forward * interactionRange);
        }
    }
}
