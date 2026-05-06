using UnityEngine;

[CreateAssetMenu(fileName = "InvAidSO", menuName = "Inventory/InvAidSO")]
public class InvAidSO : InventoryItemSO
{
    public AidSO aidData;
    public ItemType itemType = ItemType.Aid;
}
