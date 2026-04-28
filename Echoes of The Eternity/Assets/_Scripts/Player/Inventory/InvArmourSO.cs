using UnityEngine;

[CreateAssetMenu(fileName = "InvArmourSO", menuName = "Inventory/InvArmourSO")]
public class InvArmourSO : InventoryItemSO
{
    public ArmourSO armourData;
    public ItemType itemType = ItemType.Armour;
}
