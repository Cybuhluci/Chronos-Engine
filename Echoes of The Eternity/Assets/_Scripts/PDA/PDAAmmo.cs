using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PDAAmmo : MonoBehaviour
{
    public GameObject weaponButtonPrefab;
    public Transform weaponButtonParent;

    public InventoryManager inventoryManager;

    private void OnEnable()
    {
        ResetUI();

        // get the items from the inventory, and instantiate them as buttons to equip as guns.
        foreach (InventoryItemSO item in inventoryManager.GetInventoryItems())
        {
            InvAmmoSO ammoItem = item as InvAmmoSO;
            if (ammoItem == null) continue;
            var button = Instantiate(weaponButtonPrefab, weaponButtonParent);
            button.GetComponentInChildren<TMP_Text>().text = item.itemName + $" ({inventoryManager.GetAmmoTypeAmount(ammoItem.ammoType)})";
            // make sure that if an item already exists then a new button isnt made,
            // and instead the existing button gets a "(xn)" added to the end of the item name to show how many of that item there are,
            // and the transfer method is updated to use a new multi-item transfer method that allows the player to choose how many of that item they want to transfer
        }
    }

    private void ResetUI()
    {
        foreach (Transform child in weaponButtonParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void OnDisable()
    {
        ResetUI();
    }
}
