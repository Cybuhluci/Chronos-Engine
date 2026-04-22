using UnityEngine;

namespace Luci.Interactions
{
    public class PcNpcInteractScript : MonoBehaviour, IInteractable
    {
        [SerializeField] private FirstPersonController playerController;
        [SerializeField] private CharacterData characterData; // Reference to the NPC's character data

        [Header("Interaction Settings")]
        public string promptText = "NPC NAME";
        public bool isactive = true;
        public Transform npcHeadTransform; // Assign this in the inspector to the NPC's head transform

        public InventoryItemSO[] inventoryItemSOs; // Assign the NPC's inventory items in the inspector

        private void Start()
        {
            inventoryItemSOs = characterData.GetInventoryItems(); // Get the NPC's inventory items from character data
        }

        public void OnInteract(GameObject interactor)
        {
            if (!isactive) return;
            if (playerController._playerState != FirstPersonController.PlayerState.Crouching)
            {
                Debug.Log("Talking to NPC");
                DialogueManager.Instance.BeginDialogue(characterData, npcHeadTransform);
            }
            else
            {
                Debug.Log("Pickpocketing NPC");
                PickpocketManager.Instance.AttemptPickpocket(characterData, this);
            }
        }

        public string GetInteractionPrompt()
        {
            return promptText;
        }

        public InteractionType GetInteractionType()
        {
            if (playerController._playerState != FirstPersonController.PlayerState.Crouching)
            {
                return InteractionType.Talk;
            }
            else
            {
                return InteractionType.Pickpocket; 
            }
        }

        public void ToggleInteract(bool isActive)
        {
            isactive = isActive;
        }


        public void RemoveInventoryItem(InventoryItemSO item)
        {
            // Create a new array with one less element
            InventoryItemSO[] newInventory = new InventoryItemSO[inventoryItemSOs.Length - 1];
            int index = 0;
            // Copy all items except the one to remove
            for (int i = 0; i < inventoryItemSOs.Length; i++)
            {
                if (inventoryItemSOs[i] != item)
                {
                    newInventory[index] = inventoryItemSOs[i];
                    index++;
                }
            }
            // Replace the old inventory with the new one
            inventoryItemSOs = newInventory;
        }

        public void AddInventoryItem(InventoryItemSO item)
        {
            // Create a new array with one more element
            InventoryItemSO[] newInventory = new InventoryItemSO[inventoryItemSOs.Length + 1];
            // Copy existing items to the new array
            inventoryItemSOs.CopyTo(newInventory, 0);
            // Add the new item to the end of the array
            newInventory[newInventory.Length - 1] = item;
            // Replace the old inventory with the new one
            inventoryItemSOs = newInventory;
        }
    }
}