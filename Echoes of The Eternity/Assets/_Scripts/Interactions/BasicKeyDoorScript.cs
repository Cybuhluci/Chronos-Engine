using System.Linq;
using UnityEngine;

namespace Luci.Interactions
{
    public class BasicKeyDoorScript : MonoBehaviour, IInteractable
    {
        public Transform doorTransform;
        public Vector3 moveAmount;
        public Vector3 rotateAmount;
        public float speed = 2f;

        private bool isOpen = false;
        private Vector3 startPos;
        private Quaternion startRot;

        public bool requiresKey = true;
        public bool singleUseKey = false;
        public string requiredKeyName;

        void Start()
        {
            if (doorTransform == null)
                doorTransform = transform;

            startPos = doorTransform.position;
            startRot = doorTransform.rotation;
        }

        [Header("Interaction Settings")]
        public bool isHoldInteract = false;
        public string promptText = "Interact";
        public float holdDuration = 3f; // Only relevant if interactionType is Hold
        public bool isactive = true;

        public void PressInteract()
        {
            if (requiresKey)
            {
                if (PlayerInventory.Instance.HasItem(requiredKeyName))  // Check if player has the key
                {
                    Debug.Log("Key found: Opening Key Door.");
                    OpenDoor();
                    if (singleUseKey)
                    {
                        PlayerInventory.Instance.RemoveItemFromInventory(requiredKeyName);
                    }
                }
                else
                {
                    Debug.Log("Door requires key: " + requiredKeyName);
                }
            }
            else
            {
                OpenDoor();
            }
        }

        public void OnInteract(GameObject interactor)
        {
            if (!isactive) return;

            // Fire appropriate event depending on configuration
            if (!isHoldInteract)
            {
                PressInteract();
            }
            else // hold
            {
                PressInteract();
            }
        }

        public void OpenDoor()
        {
            if (!isOpen)
            {
                isOpen = true;
                StartCoroutine(MoveAndRotate(doorTransform.position + moveAmount, doorTransform.rotation * Quaternion.Euler(rotateAmount)));
            }
        }

        private System.Collections.IEnumerator MoveAndRotate(Vector3 targetPos, Quaternion targetRot)
        {
            float elapsedTime = 0;
            Vector3 initialPos = doorTransform.position;
            Quaternion initialRot = doorTransform.rotation;

            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * speed;
                doorTransform.position = Vector3.Lerp(initialPos, targetPos, elapsedTime);
                doorTransform.rotation = Quaternion.Slerp(initialRot, targetRot, elapsedTime);
                yield return null;
            }

            doorTransform.position = targetPos;
            doorTransform.rotation = targetRot;
            Destroy(gameObject);
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