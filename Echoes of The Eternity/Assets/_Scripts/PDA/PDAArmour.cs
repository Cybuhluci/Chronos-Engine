using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PDAArmour : MonoBehaviour
{
    public GameObject weaponButtonPrefab;
    public Transform weaponButtonParent;

    public ArmourManager armourManager;
    public InventoryManager inventoryManager;

    public void EquipArmourFromSO(ArmourSO Armour)
    {
        armourManager.ManageSelectedArmour(Armour);
    }

    private void OnEnable()
    {
        ResetUI();

        // get the items from the inventory, and instantiate them as buttons to equip as armour.
        var groupedArmour = inventoryManager.GetInventoryItems().OfType<InvArmourSO>().GroupBy(i => i);
        foreach (var group in groupedArmour)
        {
            var armourItem = group.Key;
            int count = group.Count();
            
            var button = Instantiate(weaponButtonPrefab, weaponButtonParent);
            button.GetComponentInChildren<TMP_Text>().text = count > 1 ? $"{armourItem.itemName} (x{count})" : armourItem.itemName;
            
            ArmourSO armourData = armourItem.armourData;
            button.GetComponent<Button>().onClick.AddListener(() => EquipArmourFromSO(armourData));
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
