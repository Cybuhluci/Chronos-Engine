using UnityEngine;

namespace Luci.Interactions
{
    public class PickupItemInteract : MonoBehaviour, IInteractable
    {
        public bool isactive = true;
        public string itemName;
        public InventoryManager playerInventory;
        public InventoryItemSO itemSO;

        public void PressInteract()
        {
            playerInventory.AddItem(itemSO);
            Destroy(gameObject);
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
            return InteractionType.Collect;
        }

        public void ToggleInteract(bool isActive)
        {
            isactive = isActive;
        }
    }
}
