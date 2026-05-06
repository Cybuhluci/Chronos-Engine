using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PDAWeapons : MonoBehaviour
{
    public GameObject weaponButtonPrefab;
    public Transform weaponButtonParent;

    public GunMainScript gunMainScript;
    public InventoryManager inventoryManager;

    public void EquipWeaponFromSO(MainGunDataSO mainGunDataSO)
    {
        gunMainScript.EquipWeaponNow(mainGunDataSO, null);
    }

    private void OnEnable()
    {
        ResetUI();

        // get the items from the inventory, and instantiate them as buttons to equip as guns.
        var groupedWeapons = inventoryManager.GetInventoryItems().OfType<InvWeaponSO>().GroupBy(i => i);
        foreach (var group in groupedWeapons)
        {
            var weaponItem = group.Key;
            int count = group.Count();

            var button = Instantiate(weaponButtonPrefab, weaponButtonParent);
            button.GetComponentInChildren<TMP_Text>().text = count > 1 ? $"{weaponItem.itemName} (x{count})" : weaponItem.itemName;
            
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

    private void OnDisable()
    {
        ResetUI();
    }
}
