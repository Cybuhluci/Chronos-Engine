using UnityEngine;
using UnityEngine.Events;

namespace Luci.Interactions
{
    public class ButtonScript : MonoBehaviour, IInteractable
    {
        [Header("Button Events")]
        public UnityEvent LeftInteraction;
        public UnityEvent RightInteraction;

        public void LeftInteract()
        {
            //Debug.Log("Button pressed!");
            LeftInteraction.Invoke();  // Calls whatever methods are assigned in the Inspector
        }

        public void RightInteract()
        {
            //Debug.Log("RightInteract");
            RightInteraction.Invoke(); // Calls whatever methods are assigned in the Inspector
        }

        public EInteractionType GetInteractionType()
        {
            return EInteractionType.InteractShort;
        }
    }
}