using UnityEngine;

[CreateAssetMenu(fileName = "InvWeaponSO", menuName = "Inventory/InvWeaponSO")]
public class InvWeaponSO : InventoryItemSO
{
    public MainGunDataSO gunData;
    public ItemType itemType = ItemType.Weapon;
}
