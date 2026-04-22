using UnityEngine;

[CreateAssetMenu(fileName = "InvWeaponSO", menuName = "Scriptable Objects/InvWeaponSO")]
public class InvWeaponSO : InventoryItemSO
{
    public MainGunDataSO gunData;
    public ItemType itemType = ItemType.Weapon;
}
