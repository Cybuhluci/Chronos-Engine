using UnityEngine;

[CreateAssetMenu(fileName = "InvAmmoSO", menuName = "Inventory/InvAmmoSO")]
public class InvAmmoSO : InventoryItemSO
{
    public AmmoTypesSO ammoType;
    public ItemType itemType = ItemType.Ammo;
}
