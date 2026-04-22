using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [SerializeField] private List<InventoryItemSO> collectedItems = new List<InventoryItemSO>();
    [SerializeField] private int inventoryWeightLimit = 100; // Maximum weight the inventory can hold before overemcumberance

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public void AddItem(InventoryItemSO item)
    {
        collectedItems.Add(item);
        Debug.Log("Item added to inventory: " + item.itemName);
    }

    public bool HasItem(InventoryItemSO item)
    {
        return collectedItems.Contains(item);
    }

    public void RemoveItem(InventoryItemSO item)
    {
        if (collectedItems.Contains(item))
        {
            collectedItems.Remove(item);
            Debug.Log("Item removed from inventory: " + item.itemName);
        }
    }

    public int GetAmmoTypeAmount(AmmoTypesSO ammoType)
    {
        int amount = 100;
        return amount;
    }

    public int GetCurrentWeight()
    {
        int totalWeight = 0;
        foreach (var item in collectedItems)
        {
            //totalWeight += item.weight;
        }
        return totalWeight;
    }

    public List<InventoryItemSO> GetInventoryItems()
    {
        return collectedItems;
    }

    public void PrintInventory()
    {
        Debug.Log("Inventory: " + string.Join(", ", collectedItems));
    }
}
