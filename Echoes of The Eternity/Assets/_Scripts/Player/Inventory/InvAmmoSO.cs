using UnityEngine;

[CreateAssetMenu(fileName = "InvAmmoSO", menuName = "Scriptable Objects/InvAmmoSO")]
public class InvAmmoSO : InventoryItemSO
{
    public AmmoTypesSO ammoType;
    public ItemType itemType = ItemType.Weapon;
}
