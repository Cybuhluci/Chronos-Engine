using UnityEngine;

namespace Luci.Interactions
{
    public class PickupItemInteract : MonoBehaviour, IInteractable
    {
        public bool isactive = true;
        public string itemName;
        public PlayerInventory playerInventory;
        public bool isHoldInteract = false;
        public float holdDuration = 0f; // Only relevant if interactionType is Hold

        public void PressInteract()
        {
            playerInventory.AddItemToInventory(this);
        }

        // Called when player interacts (presses the interact key)
        public void OnInteract(GameObject interactor)
        {
            PressInteract();
        }   

        // Short prompt to display (e.g. "Open Door" / "Pick Lock")
        public string GetInteractionPrompt()
        {
            return itemName;
        }

        // Whether this object is a press or hold interaction
        public InteractionType GetInteractionType()
        {
            return isHoldInteract ? InteractionType.Hold : InteractionType.Press;
        }

        // If interaction type is Hold, how long to hold (seconds)
        public float GetHoldDuration()
        {
            return holdDuration; // Replace with actual hold duration if needed
        }

        public void ToggleInteract(bool isActive)
        {
            isactive = isActive;
        }
    }
}
