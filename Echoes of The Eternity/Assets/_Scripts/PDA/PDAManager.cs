using UnityEngine;

public class PDAManager : MonoBehaviour
{
    public PDAWeapons pdaWeapons;
    public PDAArmour pdaArmour;
    public PDAAmmo pdaAmmo;
    public bool PDAActive = false;

    public GameObject weaponsTab; // Reference to the weapons GameObject
    public GameObject armourTab; // Reference to the armour GameObject
    public GameObject ammoTab; // Reference to the ammo GameObject
    public GameObject regularHUD; // Reference to the regular HUD GameObject
    public GameObject PDAUI; // Reference to the PDA UI GameObject

    public void TogglePDA()
    {
        PDAActive = !PDAActive;
        weaponsTab.SetActive(!PDAActive); // Show or hide the weapons based on the PDA state
        armourTab.SetActive(!PDAActive);
        ammoTab.SetActive(!PDAActive);
        regularHUD.SetActive(!PDAActive); // Show or hide the regular HUD based on the PDA state
        PDAUI.SetActive(PDAActive); // Show or hide the PDA UI based on the active state
    }

    public void OpenWeaponsTab()
    {
        weaponsTab.SetActive(true);
        CloseAmmoTab();
        CloseArmourTab();
    }

    public void CloseWeaponsTab()
    {
        weaponsTab.SetActive(false);
    }

    public void OpenArmourTab()
    {
        armourTab.SetActive(true);
        CloseWeaponsTab();
        CloseAmmoTab();
    }

    public void CloseArmourTab()
    {
        armourTab.SetActive(false);
    }
    
    public void OpenAmmoTab()
    {
        ammoTab.SetActive(true);
        CloseWeaponsTab();
        CloseArmourTab();
    }
    
    public void CloseAmmoTab()
    {
        ammoTab.SetActive(false);
    }
}