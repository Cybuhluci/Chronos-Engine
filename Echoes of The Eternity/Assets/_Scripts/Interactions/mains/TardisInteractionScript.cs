using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Luci.Interactions
{
    public enum TardisInteractionType // actually just what text the type will be, doesnt actually change much
    {
        Press,
        Hold
    }

    // Simple interactable contract. Any component that wants to be interactable can implement this.
    public interface TardisInteractable
    {
        // Called when player interacts (presses the interact key)
        void OnInteract(GameObject interactor);

        // Short prompt to display (e.g. "Open Door" / "Pick Lock")
        string GetInteractionPrompt();

        // Whether this object is a press or hold interaction
        TardisInteractionType GetInteractionType();

        // Whether this object is a press or hold interaction
        float GetHoldDuration();
    }

    public class TardisInteractionScript : MonoBehaviour
    // this script is put on the camera, so using transform.forward will be the direction the player is looking at,
    // and transform.position will be the position of the camera.
    {
        [SerializeField] private Camera playerCamera;

        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private LayerMask interactableLayer;

        [SerializeField] private Image interactionCircle; // child of the cursor parent, assigned in inspector

        [SerializeField] private GameObject cursorParent; // parent object for the cursor to keep it positioned correctly in the UI canvas
        [SerializeField] private Image InteractionTextImage; // changes size to fit the text within it.
        [SerializeField] private TMP_Text InteractionTextText; // says the name of the interaction object

        [SerializeField] private GameObject normalCursor, interactCursor, propCursor; // switch between based on interactable type.
        // interact for press, prop for hold, normal for no interacts found.

        [Header("Prompt Positioning")]
        [Tooltip("Offset from the interactable's transform position where the prompt will be spawned (world space).")]
        [SerializeField] private Vector2 cursorPosition = new Vector2(0f, 0f);

        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private Canvas uiCanvas; // optional, used to convert screen -> UI canvas coordinates

        [SerializeField] private TardisInteractable currentInteractable;
        private float _holdTimer = 0f;
        private bool _holdInProgress = false;
        private bool _holdCompleted = false;
        // no instantiation UI; we use the cursorParent UI instead


        private void Update()
        {
            CheckForInteractable();
            HandleInteractionInput();
        }

        private void CheckForInteractable()
        {
            // Use mouse position raycast when look interactions are enabled, otherwise fall back to radius checks
            RaycastHit hit = new RaycastHit();
            bool found = false;
            Ray ray = default;

            found = Physics.Raycast(transform.position, transform.forward, out hit, interactionRange, interactableLayer);

            // If we have a hit from raycast or overlap, try to get IInteractable
            if (found)
            {
                TardisInteractable interactable = null;
                Collider usedCollider = null;

                usedCollider = hit.collider;
                if (usedCollider != null)
                    interactable = usedCollider.GetComponent<TardisInteractable>();

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
                            if (cursorParent != null) cursorParent.SetActive(false);
                            if (normalCursor != null) normalCursor.SetActive(true);
                            if (interactCursor != null) interactCursor.SetActive(false);
                            if (propCursor != null) propCursor.SetActive(false);
                            InteractionTextImage.enabled = false;
                            InteractionTextText.text = "";
                            interactionCircle.fillAmount = 0f;
                        }
                        return;
                    }

                    if (currentInteractable != interactable)
                    {
                        // New target: reset any hold state
                        ResetHoldState();

                        currentInteractable = interactable;

                        // show cursor UI and populate text instead of instantiating world prompts
                        if (cursorParent != null)
                        {
                            cursorParent.SetActive(true);
                            // position via helper to account for canvas render mode
                        }

                        // select cursor sprite based on interaction type
                        var itype = TardisInteractionType.Press;
                        try { itype = currentInteractable.GetInteractionType(); } catch { itype = TardisInteractionType.Press; }
                        if (normalCursor != null) normalCursor.SetActive(false);
                        if (interactCursor != null) interactCursor.SetActive(itype == TardisInteractionType.Press);
                        if (propCursor != null) propCursor.SetActive(itype == TardisInteractionType.Hold);

                        if (InteractionTextText != null)
                        {
                            try { InteractionTextText.text = currentInteractable.GetInteractionPrompt(); } catch { InteractionTextText.text = ""; }
                        }

                        if (InteractionTextImage != null) InteractionTextImage.enabled = true;
                        if (interactionCircle != null) interactionCircle.fillAmount = 0f;
                    }
                    return;
                }
            }

            if (currentInteractable != null)
            {
                // lost focus
                currentInteractable = null;
                // clear UI prompt
                ResetHoldState();
                if (normalCursor != null) normalCursor.SetActive(true);
                if (interactCursor != null) interactCursor.SetActive(false);
                if (propCursor != null) propCursor.SetActive(false);
                if (InteractionTextImage != null) InteractionTextImage.enabled = false;
                if (InteractionTextText != null) InteractionTextText.text = "";
                if (interactionCircle != null) interactionCircle.fillAmount = 0f;
            }
        }

        private void HandleInteractionInput()
        {
            if (currentInteractable == null) return;

            // determine interaction type
            TardisInteractionType type = TardisInteractionType.Press;
            try { type = currentInteractable.GetInteractionType(); } catch { type = TardisInteractionType.Press; }

            // Input System action if present
            var hasInputSystem = playerInput != null && playerInput.actions["ClickInteract"] != null;

            if (type == TardisInteractionType.Press)
            {
                bool interactPressed = false;
                if (hasInputSystem)
                {
                    interactPressed = playerInput.actions["ClickInteract"].WasPressedThisFrame();
                }
                else
                {
                    interactPressed = Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F);
                }

                if (interactPressed)
                {
                    currentInteractable.OnInteract(gameObject);
                }
            }
            else // Hold
            {
                bool isPressed = false;
                bool wasReleased = false;
                if (hasInputSystem)
                {
                    var action = playerInput.actions["ClickInteract"];
                    isPressed = action.IsPressed();
                    wasReleased = action.WasReleasedThisFrame();
                    // note: WasPressedThisFrame could be used to start, but IsPressed covers that.
                }

                if (isPressed && !_holdCompleted)
                {
                    if (!_holdInProgress)
                    {
                        _holdInProgress = true;
                        _holdTimer = 0f;
                        // show UI start
                        if (interactionCircle != null) interactionCircle.fillAmount = 0f;
                    }

                    _holdTimer += Time.deltaTime;
                    float required = 1f;
                    try { required = currentInteractable.GetHoldDuration(); } catch { required = 1f; }
                    if (interactionCircle != null) interactionCircle.fillAmount = Mathf.Clamp01(_holdTimer / required);

                    if (_holdTimer >= required)
                    {
                        // complete
                        currentInteractable.OnInteract(gameObject);
                        _holdCompleted = true;
                        _holdInProgress = false;
                    }
                }

                // if released before completion, cancel
                if (wasReleased && !_holdCompleted)
                {
                    ResetHoldState();
                }

                // reset when fully released after completion
                if (wasReleased && _holdCompleted)
                {
                    ResetHoldState();
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
        private bool IsInteractableActive(TardisInteractable interactable)
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
            f = t.GetField("isActive");
            if (f != null && f.FieldType == typeof(bool))
            {
                return (bool)f.GetValue(mb);
            }

            var p = t.GetProperty("isActive");
            if (p != null && p.PropertyType == typeof(bool))
            {
                return (bool)p.GetValue(mb);
            }
            p = t.GetProperty("IsActive");
            if (p != null && p.PropertyType == typeof(bool))
            {
                return (bool)p.GetValue(mb);
            }

            // default: treat as active
            return true;
        }

        // Useful debug visualization in the editor
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, transform.forward * interactionRange);
        }
    }
}