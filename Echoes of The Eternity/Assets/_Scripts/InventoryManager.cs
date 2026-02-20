using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [SerializeField] private List<string> collectedItems = new List<string>();  // Now visible in Inspector

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddItem(string itemName)
    {
        collectedItems.Add(itemName);
        Debug.Log("Item added to inventory: " + itemName);
    }

    public bool HasItem(string itemName)
    {
        return collectedItems.Contains(itemName);
    }

    public void RemoveItem(string itemName)
    {
        if (collectedItems.Contains(itemName))
        {
            collectedItems.Remove(itemName);
            Debug.Log("Item removed from inventory: " + itemName);
        }
    }

    public void PrintInventory()
    {
        Debug.Log("Inventory: " + string.Join(", ", collectedItems));
    }
}
