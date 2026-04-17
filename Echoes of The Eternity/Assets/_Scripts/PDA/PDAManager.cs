using UnityEngine;

public class PDAManager : MonoBehaviour
{
    public PDAWeapons pdaWeapons;
    public bool PDAActive = false;

    public GameObject weapons; // Reference to the weapons GameObject
    public GameObject regularHUD; // Reference to the regular HUD GameObject
    public GameObject PDAUI; // Reference to the PDA UI GameObject

    public void TogglePDA()
    {
        PDAActive = !PDAActive;
        weapons.SetActive(!PDAActive); // Show or hide the weapons based on the PDA state
        regularHUD.SetActive(!PDAActive); // Show or hide the regular HUD based on the PDA state
        PDAUI.SetActive(PDAActive); // Show or hide the PDA UI based on the active state
    }
}