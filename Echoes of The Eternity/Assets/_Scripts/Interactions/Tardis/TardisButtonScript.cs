using UnityEngine;
using UnityEngine.Events;

namespace Luci.Interactions
{
    public class TardisButtonScript : MonoBehaviour, TardisInteractable
    {
        [Header("Interaction Settings")]
        public string promptText = "Interact";
        public bool isactive = true;
        public bool isHoldInteraction = false;
        public float holdDuration = 1f;

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
        public TardisInteractionType GetInteractionType()
        {
            return isHoldInteraction ? TardisInteractionType.Hold : TardisInteractionType.Press;
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