using Luci.Saving;
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
            SaveInventoryToFile();
            Debug.Log("Item removed from inventory: " + item.itemName);
        }
    }

    public void ClearInventory()
    {
        collectedItems.Clear();
    }

    public int GetAmmoTypeAmount(AmmoTypesSO ammoType)
    {
        int amount = 0;

        // only search for invammoso - get the quantity count held in the inventory for that ammo type
        foreach (var item in collectedItems)
        {
            if (item is InvAmmoSO ammoItem && ammoItem.ammoType == ammoType)
            {
                amount++;
            }
        }

        return amount;
    }

    public void ConsumeAmmo(AmmoTypesSO ammoType, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            for (int j = 0; j < collectedItems.Count; j++)
            {
                if (collectedItems[j] is InvAmmoSO ammoItem && ammoItem.ammoType == ammoType)
                {
                    collectedItems.RemoveAt(j);
                    break;
                }
            }
        }
        SaveInventoryToFile();
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

    public bool IsOverEncumbered()
    {
        return GetCurrentWeight() > inventoryWeightLimit;
    }

    public void SaveInventoryToFile()
    {
        SaveManager.Instance.SaveInventory(collectedItems);
    }

    public void LoadInventoryFromFile()
    {
        SaveManager.Instance.LoadInventory();
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
