using System.Linq;
using TMPro;
using UnityEngine;

public class PDAMisc : MonoBehaviour
{
    public GameObject weaponButtonPrefab;
    public Transform weaponButtonParent;

    //public MiscManager miscManager;
    public InventoryManager inventoryManager;

    private void OnEnable()
    {
        ResetUI();

        // get the items from the inventory, and instantiate them as buttons to equip as armour.
        var groupedArmour = inventoryManager.GetInventoryItems().OfType<InvMiscSO>().GroupBy(i => i);
        foreach (var group in groupedArmour)
        {
            var miscItem = group.Key;
            int count = group.Count();

            var button = Instantiate(weaponButtonPrefab, weaponButtonParent);
            button.GetComponentInChildren<TMP_Text>().text = count > 1 ? $"{miscItem.itemName} (x{count})" : miscItem.itemName;
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
