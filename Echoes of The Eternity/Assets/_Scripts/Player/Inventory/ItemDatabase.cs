using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ItemDatabase
{
    private static Dictionary<string, InventoryItemSO> items;
    public static bool IsInitialized { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        items = new Dictionary<string, InventoryItemSO>();
        var allItems = Resources.LoadAll<InventoryItemSO>("");
        foreach (var item in allItems)
        {
            if (!items.ContainsKey(item.id))
            {
                items.Add(item.id, item);
            }
            else
            {
                Debug.LogWarning($"Duplicate item ID found: {item.id}");
            }
        }
        IsInitialized = true;
        Debug.Log($"Item Database Initialized with {items.Count} items.");
    }

    public static bool GetItem(string id, out InventoryItemSO item)
    {
        item = null;
        if (items == null || string.IsNullOrEmpty(id))
        {
            return false;
        }
        return items.TryGetValue(id, out item);
    }
}
