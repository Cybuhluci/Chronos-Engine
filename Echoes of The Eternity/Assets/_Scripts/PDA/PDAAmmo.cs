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
