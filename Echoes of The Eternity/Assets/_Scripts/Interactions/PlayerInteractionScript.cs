using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Luci.Interactions
{
    public enum InteractionType
    {
        Press,
        Hold,
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

        // If interaction type is Hold, how long to hold (seconds)
        float GetHoldDuration();
    }

    public class PlayerInteractionScript : MonoBehaviour
        // this script is put on the camera, so using transform.forward will be the direction the player is looking at,
        // and transform.position will be the position of the camera.
    {
        // this bool can be added upon after basic look interactions are implemented and work
        [SerializeField] private bool lookInteractables = true; // if false, the player can interact with objects in a radius around them instead of looking at them

        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private LayerMask interactableLayer;

        [SerializeField] private GameObject instantiateInteractionPromt; // a prefab which should be instantiated above the interactable object
        [SerializeField] private Image interactionCircle; // child of instantiateInteractionPromt
        [SerializeField] private TMP_Text interactionText; // child of instantiateInteractionPromt

        [Header("Prompt Positioning")]
        [Tooltip("Offset from the interactable's transform position where the prompt will be spawned (world space).")]
        [SerializeField] private Vector3 promptOffset = new Vector3(0f, 0.25f, 0f);

        [SerializeField] private PlayerInput playerInput;

        private IInteractable currentInteractable;
        private float _holdTimer = 0f;
        private bool _holdInProgress = false;
        private bool _holdCompleted = false;
        private GameObject _spawnedPrompt;

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
                    if (currentInteractable != interactable)
                    {
                        // New target: reset any hold state
                        ResetHoldState();
                        // Clean up existing prompt if any
                        if (_spawnedPrompt != null)
                        {
                            Destroy(_spawnedPrompt);
                            _spawnedPrompt = null;
                        }

                        currentInteractable = interactable;

                        // instantiate prompt prefab (if provided) and try to find the Image inside
                        if (instantiateInteractionPromt != null)
                        {
                            // Determine spawn position using configurable offset relative to the interactable's transform
                            Vector3 spawnPos;
                            if (hit.collider != null)
                                spawnPos = hit.collider.transform.position + promptOffset;
                            else
                                spawnPos = hit.point + promptOffset;

                            _spawnedPrompt = Instantiate(instantiateInteractionPromt, spawnPos, Quaternion.identity);
                            if (hit.collider != null)
                            {
                                // parent so the prompt follows the object; keep world position
                                _spawnedPrompt.transform.SetParent(hit.collider.transform, true);
                            }

                            // Find an Image in the instantiated prefab to use as the interaction circle
                            var img = _spawnedPrompt.GetComponentInChildren<Image>();
                            if (img != null)
                            {
                                interactionCircle = img;
                                interactionCircle.fillAmount = 0f;
                            }
                            // Find a TMP_Text in the instantiated prefab to use as the interaction prompt text
                            var tmp = _spawnedPrompt.GetComponentInChildren<TMP_Text>();
                            if (tmp != null)
                            {
                                interactionText = tmp;
                                // initialize text from the interactable
                                try { interactionText.text = currentInteractable.GetInteractionPrompt(); } catch { interactionText.text = ""; }
                            }
                        }
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
                if (_spawnedPrompt != null)
                {
                    Destroy(_spawnedPrompt);
                    _spawnedPrompt = null;
                }
                interactionCircle = null;
                interactionText = null;
            }
        }

        private void HandleInteractionInput()
        {
            if (currentInteractable == null) return;

            // determine interaction type
            InteractionType type = InteractionType.Press;
            try { type = currentInteractable.GetInteractionType(); } catch { type = InteractionType.Press; }

            // Input System action if present
            var hasInputSystem = playerInput != null && playerInput.actions["Interact"] != null;

            if (type == InteractionType.Press)
            {
                bool interactPressed = false;
                if (hasInputSystem)
                {
                    interactPressed = playerInput.actions["Interact"].WasPressedThisFrame();
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
                    var action = playerInput.actions["Interact"];
                    isPressed = action.IsPressed();
                    wasReleased = action.WasReleasedThisFrame();
                    // note: WasPressedThisFrame could be used to start, but IsPressed covers that.
                }
                else
                {
                    isPressed = Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.F);
                    wasReleased = Input.GetKeyUp(KeyCode.E) || Input.GetKeyUp(KeyCode.F);
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

        // Useful debug visualization in the editor
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, transform.forward * interactionRange);
        }
    }
}
