//using UnityEngine;
//using UnityEngine.InputSystem;
//using System.Collections.Generic;
//using TMPro;

//public enum InteractType
//{
//    Pickup,
//    Door,
//    MysteryBox,
//    Purchasable, // "buy" interaction
//    Other
//}

//public interface IInteractable
//{
//    // along with pressinteract, we need a way to also turn on and off the interaction entirely.
//    // this would be for: active mystery box, and already owned perks.
//    void PressInteract();
//    void ToggleInteract(bool isActive);
//}

//namespace Luci.Interactions
//{
//    public class PlayerColliderInteract : MonoBehaviour
//    {
//        public LayerMask interactableLayer;
//        public PlayerInput playerInput;

//        public TMP_Text InteractTextUI;

//        public float interactionRadius = 3f;
//        public List<IInteractable> interactableList = new List<IInteractable>();
//        public List<Transform> interactableTransforms = new List<Transform>();
//        public int currentIndex = 0;

//        public IInteractable currentInteractable;
//        public Transform currentTargetTransform;

//        // Hold interact support
//        private float holdTimer = 0f;
//        private const float holdDuration = 0.2f;
//        private bool isHolding = false;
//        private bool hasInteracted = false;

//        void Start()
//        {
//            Cursor.lockState = CursorLockMode.Locked;
//        }

//        void Update()
//        {
//            if (PlayerPrefs.GetInt("CameraDisable", 0) == 1) return;

//            UpdateNearbyInteractables();
//            HandleInteractionInput();
//            UpdateInteractText();
//        }

//        void UpdateNearbyInteractables()
//        {
//            interactableList.Clear();
//            interactableTransforms.Clear();

//            Collider[] hits = Physics.OverlapSphere(transform.position, interactionRadius, interactableLayer);

//            foreach (Collider collider in hits)
//            {
//                IInteractable interactable = collider.GetComponent<IInteractable>();
//                if (interactable != null)
//                {
//                    // Check if the interactable is active by calling ToggleInteract(false) and expecting a bool return, or use a public property
//                    // We'll use reflection to check for a public bool 'isActive' field or property
//                    var mb = interactable as MonoBehaviour;
//                    bool isActive = true;
//                    if (mb != null)
//                    {
//                        var isActiveField = mb.GetType().GetField("isactive");
//                        if (isActiveField != null)
//                        {
//                            isActive = (bool)isActiveField.GetValue(mb);
//                        }
//                        else
//                        {
//                            var isActiveProp = mb.GetType().GetProperty("isactive");
//                            if (isActiveProp != null)
//                            {
//                                isActive = (bool)isActiveProp.GetValue(mb);
//                            }
//                        }
//                    }
//                    if (isActive)
//                    {
//                        interactableList.Add(interactable);
//                        interactableTransforms.Add(collider.transform);
//                    }
//                }
//            }

//            // Ensure index is valid
//            if (interactableList.Count == 0)
//            {
//                currentInteractable = null;
//                currentTargetTransform = null;
//                currentIndex = 0;
//            }
//            else
//            {
//                // Clamp the index and set the current interactable
//                currentIndex = Mathf.Clamp(currentIndex, 0, interactableList.Count - 1);
//                currentInteractable = interactableList[currentIndex];
//                currentTargetTransform = interactableTransforms[currentIndex];
//            }
//        }

//        void HandleInteractionInput()
//        {
//            if (currentInteractable != null)
//            {
//                MonoBehaviour mb = currentInteractable as MonoBehaviour;
//                if (mb != null)
//                {
//                    var holdField = mb.GetType().GetField("isHoldInteract");
//                    bool isHold = holdField != null && (bool)holdField.GetValue(mb);
//                    var interactAction = playerInput.actions["Interact"];

//                    if (isHold)
//                    {
//                        if (interactAction.IsPressed())
//                        {
//                            holdTimer += Time.deltaTime;
//                            isHolding = true;
//                            if (!hasInteracted && holdTimer >= holdDuration)
//                            {
//                                currentInteractable.PressInteract();
//                                hasInteracted = true;
//                            }
//                        }
//                        else
//                        {
//                            holdTimer = 0f;
//                            isHolding = false;
//                            hasInteracted = false;
//                        }
//                    }
//                    else
//                    {
//                        if (interactAction.WasPressedThisFrame())
//                        {
//                            currentInteractable.PressInteract();
//                        }
//                        holdTimer = 0f;
//                        isHolding = false;
//                        hasInteracted = false;
//                    }
//                }
//            }
//            else
//            {
//                holdTimer = 0f;
//                isHolding = false;
//                hasInteracted = false;
//            }
//        }

//        void UpdateInteractText()
//        {
//            if (currentInteractable != null)
//            {
//                MonoBehaviour mb = currentInteractable as MonoBehaviour;
//                if (mb != null)
//                {
//                    var promptField = mb.GetType().GetField("interactPrompt");
//                    var typeField = mb.GetType().GetField("interactType");
//                    var priceField = mb.GetType().GetField("cost");
//                    var holdField = mb.GetType().GetField("isHoldInteract");

//                    string prompt = promptField != null ? (string)promptField.GetValue(mb) : null;
//                    InteractType type = typeField != null ? (InteractType)typeField.GetValue(mb) : InteractType.Other;
//                    int price = priceField != null ? (int)priceField.GetValue(mb) : 0;
//                    bool isHold = holdField != null && (bool)holdField.GetValue(mb);

//                    string objectName = "";

//                    string actionText = isHold ? "Hold F" : "Press F";
//                    string typePrompt = "";

//                    if (string.IsNullOrEmpty(prompt))
//                    {
//                        switch (type)
//                        {
//                            case InteractType.Pickup:
//                                typePrompt = "pickup";
//                                break;
//                            case InteractType.Door:
//                                typePrompt = "open door"; // has cost
//                                break;
//                            case InteractType.MysteryBox:
//                                typePrompt = "use Mystery Box"; // has cost
//                                break;
//                            case InteractType.Purchasable:
//                                typePrompt = "buy"; // has cost
//                                break;
//                            default:
//                                typePrompt = "interact";
//                                break;
//                        }
//                    }

//                    string text;
//                    if (!string.IsNullOrEmpty(prompt))
//                    {
//                        // If prompt already contains "Press F" or "Hold F", use as is
//                        if (prompt.ToLower().Contains("press f") || prompt.ToLower().Contains("hold f"))
//                            text = prompt;
//                        else
//                            text = $"{actionText} to {prompt}";
//                    }
//                    else
//                    {
//                        text = $"{actionText} to {typePrompt}";
//                    }

//                    // For Purchasables, append object name (perk or weapon)
//                    if (type == InteractType.Purchasable && !string.IsNullOrEmpty(objectName))
//                        text += $" {objectName}";

//                    if (price > 0)
//                        text += $" [Cost: {price}]";

//                    InteractTextUI.text = text;
//                    InteractTextUI.enabled = true;
//                }
//                else
//                {
//                    InteractTextUI.text = "";
//                    InteractTextUI.enabled = false;
//                }
//            }
//            else
//            {
//                InteractTextUI.text = "";
//                InteractTextUI.enabled = false;
//            }
//        }

//        void OnDrawGizmosSelected()
//        {
//            Gizmos.color = Color.yellow;
//            Gizmos.DrawWireSphere(transform.position, interactionRadius);
//        }
//    }
//}