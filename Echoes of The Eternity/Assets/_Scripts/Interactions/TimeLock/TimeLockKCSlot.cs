using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Luci.Interactions
{
    public class TimeLockKCSlot : MonoBehaviour, IInteractable
    {
        [SerializeField] private TimeLockDoor linkedDoor; // Reference to the door this slot is linked to

        [Header("Interaction Settings")]
        public bool isHoldInteract = false;
        public string promptText = "Insert Keycard";
        public float holdDuration = 0f; // Only relevant if interactionType is Hold
        public bool isactive = true;

        [SerializeField] private AudioSource insertSound; // Sound to play when keycard is inserted

        public void OnInteract(GameObject interactor)
        {
            if (!isactive) return;

            // Fire appropriate event depending on configuration
            if (!isHoldInteract)
            {
                InsertKeycard();
            }
            else // hold
            {
                HackTimeLock();
            }
        }

        private void InsertKeycard()
        {
            if (PlayerInventory.Instance.GetInventory().Contains("Keycard"))
            {
                PlayerInventory.Instance.RemoveItemFromInventory("Keycard");
                insertSound.Play();
                linkedDoor.InsertKeycard();
                isactive = false; // Disable further interaction after inserting the keycard
            }
        }

        private void HackTimeLock()
        {
            // Placeholder for hacking logic - start a timer
            Debug.Log("Hacking Time Lock...");
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