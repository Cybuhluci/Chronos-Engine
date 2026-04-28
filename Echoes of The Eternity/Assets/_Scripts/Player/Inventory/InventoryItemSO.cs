using UnityEngine;

public class InventoryItemSO : ScriptableObject
{
    public string itemName;

    public enum ItemType
    {
        Weapon,
        Armour,
        Aid,
        Misc,
        Ammo
    }
}
