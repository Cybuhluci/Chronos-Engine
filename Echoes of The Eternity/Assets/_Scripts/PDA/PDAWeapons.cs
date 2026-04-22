using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PDAWeapons : MonoBehaviour
{
    public GameObject weaponButtonPrefab;
    public Transform weaponButtonParent;

    public GunMainScript gunMainScript;
    public InventoryManager inventoryManager;
    public GameObject inventoryItemParent;
    public GameObject itemButtonPrefab;

    public void EquipWeaponFromSO(MainGunDataSO mainGunDataSO)
    {
        gunMainScript.EquipWeaponNow(mainGunDataSO, null);
    }

    private void OnEnable()
    {
        ResetUI();

        // get the items from the inventory, and instantiate them as buttons to equip as guns.
        foreach (InventoryItemSO item in inventoryManager.GetInventoryItems())
        {
            InvWeaponSO weaponItem = item as InvWeaponSO;
            if (weaponItem == null) continue;
            var button = Instantiate(weaponButtonPrefab, weaponButtonParent);
            button.GetComponentInChildren<TMP_Text>().text = item.itemName;
            MainGunDataSO gunData = weaponItem.gunData;
            button.GetComponent<Button>().onClick.AddListener(() => EquipWeaponFromSO(gunData));
        }
    }

    private void ResetUI()
    {
        foreach (Transform child in weaponButtonParent)
        {
            Destroy(child.gameObject);
        }
    }
}
