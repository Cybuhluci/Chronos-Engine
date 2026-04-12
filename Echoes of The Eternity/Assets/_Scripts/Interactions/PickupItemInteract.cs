using System.Linq;
using UnityEngine;

namespace Luci.Interactions
{
    public class PickupItemInteract : MonoBehaviour, IInteractable
    {
        public bool isactive = true;
        public string itemName;
        public PlayerInventory playerInventory;

        public void PressInteract()
        {
            playerInventory.AddItemToInventory(this);
            Destroy(gameObject);
        }

        private void Update()
        {
            if (playerInventory.GetInventory().Contains(itemName))
            {
                isactive = false;
            }
            else 
            {                 
                isactive = true;
            }
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
            return InteractionType.Interact;
        }

        public void ToggleInteract(bool isActive)
        {
            isactive = isActive;
        }
    }
}
