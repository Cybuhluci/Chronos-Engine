using UnityEngine;
using UnityEngine.Events;

namespace Luci.Interactions
{
    public class ButtonScript : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        public string promptText = "Interact";
        public bool isactive = true;

        [Header("Button Events")]
        public UnityEvent RegularInteraction;

        public void OnInteract(GameObject interactor)
        {
            if (!isactive) return;

            RegularInteraction?.Invoke();
        }

        public string GetInteractionPrompt()
        {
            return promptText;
        }

        // Whether this object is a press or hold interaction
        public InteractionType GetInteractionType()
        {
            return InteractionType.Interact;
        }

        public void ToggleInteract(bool isActive)
        {
            isactive = isActive;
        }
    }
}