using UnityEngine;

public class PDAManager : MonoBehaviour
{
    public PDAWeapons pdaWeapons;
    public PDAArmour pdaArmour;
    public PDAAmmo pdaAmmo;
    public bool PDAActive = false;

    public GameObject weaponsTab; // Reference to the weapons GameObject
    public GameObject armourTab; // Reference to the armour GameObject
    public GameObject aidTab; // Reference to the aid GameObject
    public GameObject miscTab; // Reference to the misc GameObject
    public GameObject ammoTab; // Reference to the ammo GameObject
    public GameObject regularHUD; // Reference to the regular HUD GameObject
    public GameObject PDAUI; // Reference to the PDA UI GameObject

    public void TogglePDA()
    {
        PDAActive = !PDAActive;
        weaponsTab.SetActive(!PDAActive); // Show or hide the weapons based on the PDA state
        armourTab.SetActive(!PDAActive);
        ammoTab.SetActive(!PDAActive);
        aidTab.SetActive(!PDAActive);
        miscTab.SetActive(!PDAActive);
        regularHUD.SetActive(!PDAActive); // Show or hide the regular HUD based on the PDA state
        PDAUI.SetActive(PDAActive); // Show or hide the PDA UI based on the active state
    }

    private void CloseTabs()
    {
        CloseWeaponsTab();
        CloseArmourTab();
        CloseAmmoTab();
        CloseMiscTab();
        CloseAidTab();
    }

    public void OpenWeaponsTab()
    {
        CloseTabs();
        weaponsTab.SetActive(true);
    }

    public void CloseWeaponsTab()
    {
        weaponsTab.SetActive(false);
    }

    public void OpenArmourTab()
    {
        CloseTabs();
        armourTab.SetActive(true);
    }

    public void CloseArmourTab()
    {
        armourTab.SetActive(false);
    }

    public void OpenAidTab()
    {
        CloseTabs();
        aidTab.SetActive(true);
    }

    public void CloseAidTab()
    {
        aidTab.SetActive(false);
    }

    public void OpenMiscTab()
    {
        CloseTabs();
        miscTab.SetActive(true);
    }

    public void CloseMiscTab()
    {
        miscTab.SetActive(false);
    }

    public void OpenAmmoTab()
    {
        CloseTabs();
        ammoTab.SetActive(true);
    }
    
    public void CloseAmmoTab()
    {
        ammoTab.SetActive(false);
    }
}