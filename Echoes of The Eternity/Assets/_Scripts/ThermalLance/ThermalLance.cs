using System.Collections;
using UnityEngine;

namespace Luci.Interactions
{
    public class ThermalLance : MonoBehaviour, IInteractable
    {
        public GameObject theDrill;

        public int drillTime = 10;
        public int breaks = 2;

        public bool isactive = true;
        public string itemName;
        public static bool isHoldInteract = true;
        public float holdDuration = 5f; // Only relevant if interactionType is Hold

        public void PressInteract()
        {

        }

        public void ActivateLance(Transform location, BasicKeyDoorScript door)
        {
            StartCoroutine(DrillCoroutine(door));
        }

        IEnumerator DrillCoroutine(BasicKeyDoorScript door)
        {
            // countdown the drill time, and every now and again try to break the drill, if the break succeeds, pause drill timer and wait for interaction repair, if the drill timer hits 0, open the door
            yield return new WaitForSeconds(holdDuration);
            door.OpenDoor();
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