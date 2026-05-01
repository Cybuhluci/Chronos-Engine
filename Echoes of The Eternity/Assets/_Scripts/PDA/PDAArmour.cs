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

        // get the items from the inventory, and instantiate them as buttons to equip as guns.
        foreach (InventoryItemSO item in inventoryManager.GetInventoryItems())
        {
            InvArmourSO armourItem = item as InvArmourSO;
            if (armourItem == null) continue;
            var button = Instantiate(weaponButtonPrefab, weaponButtonParent);
            button.GetComponentInChildren<TMP_Text>().text = item.itemName;
            ArmourSO armourData = armourItem.armourData;
            button.GetComponent<Button>().onClick.AddListener(() => EquipArmourFromSO(armourData));
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
