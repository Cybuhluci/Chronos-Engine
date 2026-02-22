using Luci.Interactions;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public void AddItemToInventory(PickupItemInteract item)
    {
        Debug.Log("Item added to inventory: " + item.itemName);
    }
}
