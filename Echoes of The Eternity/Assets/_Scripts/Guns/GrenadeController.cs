using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GrenadeController : MonoBehaviour
{
    public GrenadeDataSO grenadeData; // Reference to the ScriptableObject containing the gadget's data
    [SerializeField] int currentAmmo; // Current ammo count for the gadget
    [SerializeField] Image chunkImage; // Reference to the UI image representing the ammo chunk
    [SerializeField] TMP_Text GrenadeAmmo;

    [SerializeField] GameObject UIInstance;
    [SerializeField] GameObject UIInstanceContainer;

    public void StartGrenade()
    {
        currentAmmo = grenadeData.maxAmmo; // Initialize current ammo to max ammo from the ScriptableObject

        UIInstanceContainer = GameObject.FindWithTag("WeaponInstantHUD");

        if (UIInstanceContainer != null)
        {
            GameObject instantiatedUI = Instantiate(UIInstance, UIInstanceContainer.transform);
            GrenadeAmmo = instantiatedUI.GetComponentInChildren<TMP_Text>();
            chunkImage = instantiatedUI.GetComponentInChildren<Image>();
        }

        UpdateAmmoUI();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAmmoUI();
    }

    public void UpdateAmmoUI()
    {
        if (GrenadeAmmo != null)
        {
            GrenadeAmmo.text = currentAmmo.ToString();
        }
    }

    public void FillChunkAmmo(float percentage)
    {
        // fills the chunkImage, when the image is full, adds 1 ammo.
        if (chunkImage != null)
        {
            chunkImage.fillAmount += percentage;
            if (chunkImage.fillAmount >= 1f)
            {
                chunkImage.fillAmount = 0f;
                currentAmmo++;
            }
        }
    }
}
