using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PDAAid : MonoBehaviour
{
    public GameObject weaponButtonPrefab;
    public Transform weaponButtonParent;

    public AidManager aidManager;
    public InventoryManager inventoryManager;

    public void UseAidItem(AidSO aid)
    {
        aidManager.UseAidIem(aid);
    }

    private void OnEnable()
    {
        ResetUI();

        // get the items from the inventory, and instantiate them as buttons to equip as armour.
        var groupedArmour = inventoryManager.GetInventoryItems().OfType<InvAidSO>().GroupBy(i => i);
        foreach (var group in groupedArmour)
        {
            var aidItem = group.Key;
            int count = group.Count();

            var button = Instantiate(weaponButtonPrefab, weaponButtonParent);
            button.GetComponentInChildren<TMP_Text>().text = count > 1 ? $"{aidItem.itemName} (x{count})" : aidItem.itemName;

            AidSO aidData = aidItem.aidData;
            button.GetComponent<Button>().onClick.AddListener(() => UseAidItem(aidData));
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
