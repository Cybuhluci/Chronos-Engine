using Luci.Interactions;
using UnityEngine;

namespace Luci.Interactions
{
    public class BagPickupScript : MonoBehaviour, IInteractable
    {
        public bool IsLootBag = true;
        public LootSO lootSO;
        public GameObject miscBag;
        public PlayerBagScript playerBagScript;
        public bool isactive = true;
        public string itemName;

        private void Start()
        {
            playerBagScript = FindFirstObjectByType<PlayerBagScript>();
        }

        private void Update()
        {
            isactive = playerBagScript.CanAddBag();
        }

        public void PressInteract()
        {
            if (IsLootBag)
            {
                playerBagScript.AddLoot(lootSO);
                Destroy(gameObject);
            }
            else
            {
                playerBagScript.AddMiscBag(miscBag);
                Destroy(gameObject);
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