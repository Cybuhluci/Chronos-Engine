using UnityEngine;

public class PropScript : MonoBehaviour, IInteractable
{
    public string itemName;  // Name of the item

    public void Interact()
    {
        Debug.Log("holding item");
        //InventoryManager.Instance.AddItem(itemName);  // Add to inventory
        //Destroy(gameObject);  // Remove from the scene

        //if (itemName == "TARDISkey")
        //{
        //    PlayerPrefs.SetInt("HasTardisKey", 1);
        //}
    }

    public EInteractionType GetInteractionType()
    {
        return EInteractionType.InteractShort; // Returns the enum value for a short interaction
    }
}
