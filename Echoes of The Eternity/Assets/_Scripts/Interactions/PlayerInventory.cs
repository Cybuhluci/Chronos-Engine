using Luci.Interactions;
using System.Linq;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddItemToInventory(PickupItemInteract item)
    {
        var newInventory = new string[inventory.Length + 1];
        inventory.CopyTo(newInventory, 0);
        newInventory[newInventory.Length - 1] = item.itemName;
        inventory = newInventory;
    }

    public bool HasItem(string itemName)
    {
        return inventory.Contains(itemName); 
    }

    public void RemoveItemFromInventory(string itemName)
    {
        var newInventory = new string[inventory.Length - 1];
        int index = 0;
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] != itemName)
            {
                newInventory[index] = inventory[i];
                index++;
            }
        }
        inventory = newInventory;
    }

    public string[] inventory;

    public string[] GetInventory()
    {
        return inventory;
    }
}
