using UnityEngine;
using UnityEngine.Events;

namespace Luci.Interactions
{
    public class ButtonScript : MonoBehaviour, IInteractable
    {
        [Header("Button Events")]
        public UnityEvent RegularInteraction;
        public UnityEvent ModifierInteraction;

        public void RegularInteract()
        {
            //Debug.Log("Button pressed!");
            RegularInteraction.Invoke();  // Calls whatever methods are assigned in the Inspector
        }

        public void ModifierInteract()
        {
            //Debug.Log("Modifier interaction!");
            ModifierInteraction.Invoke(); // Calls whatever methods are assigned in the Inspector
        }

        public EInteractionType GetInteractionType()
        {
            return EInteractionType.InteractShort;
        }
    }
}