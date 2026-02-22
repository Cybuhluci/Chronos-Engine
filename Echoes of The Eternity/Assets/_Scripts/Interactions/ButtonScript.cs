using UnityEngine;
using UnityEngine.Events;

namespace Luci.Interactions
{
    public class ButtonScript : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        public bool isHoldInteract = false;
        public string promptText = "Interact";
        public float holdDuration = 3f; // Only relevant if interactionType is Hold
        public bool isactive = true;

        [Header("Button Events")]
        public UnityEvent RegularInteraction;
        public UnityEvent ModifierInteraction;

        public void OnInteract(GameObject interactor)
        {
            if (!isactive) return;

            // Fire appropriate event depending on configuration
            if (!isHoldInteract)
            {
                RegularInteraction?.Invoke();
            }
            else // hold
            {
                ModifierInteraction?.Invoke();
            }
        }

        public string GetInteractionPrompt()
        {
            return promptText;
        }

        // Whether this object is a press or hold interaction
        public InteractionType GetInteractionType()
        {
            return isHoldInteract ? InteractionType.Hold : InteractionType.Press;
        }

        public float GetHoldDuration()
        {
            return holdDuration;
        }

        public void ToggleInteract(bool isActive)
        {
            isactive = isActive;
        }
    }
}