using UnityEngine;

namespace Luci.Interactions
{
    public class PropScript : MonoBehaviour, IInteractable
    {
        public string itemName;  // Name of the item

        public void LeftInteract()
        {
            Debug.Log("LeftInteract");
        }

        public void RightInteract()
        {
            Debug.Log("RightInteract");
        }

        public EInteractionType GetInteractionType()
        {
            return EInteractionType.InteractShort; // Returns the enum value for a short interaction
        }
    }
}
