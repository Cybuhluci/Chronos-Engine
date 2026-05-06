using System.Linq;
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
        var groupedAmmo = inventoryManager.GetInventoryItems().OfType<InvAmmoSO>().GroupBy(i => i);
        foreach (var group in groupedAmmo)
        {
            var ammoItem = group.Key;
            
            var button = Instantiate(weaponButtonPrefab, weaponButtonParent);
            button.GetComponentInChildren<TMP_Text>().text = $"{ammoItem.itemName} ({inventoryManager.GetAmmoTypeAmount(ammoItem.ammoType)})";
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
