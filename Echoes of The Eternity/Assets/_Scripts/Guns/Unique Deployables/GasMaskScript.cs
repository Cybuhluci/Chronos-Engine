using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GasMaskScript : UniqueDeployableMainScript
{
    [SerializeField] UniqueDeployableSO gasMaskData; // Reference to the gas mask's data from the UniqueDeployableSO
    [SerializeField] TMP_Text ChargesText;
    [SerializeField] GameObject gasMaskOverlay; // Reference to the gas mask overlay UI element
    [SerializeField] Image gasMaskCooldown; // Reference to the gas mask cooldown image component

    [SerializeField] int chargesRemaining; // Current number of charges remaining in the gas mask

    [SerializeField] bool _isGasMaskActive; // Flag to track whether the gas mask is currently active

    private void Start()
    {
        chargesRemaining = gasMaskData.startingAmmo;
    }

    public override void ToggleUniqueDeployable()
    {
        ToggleUniqueDeploy();
    }

    public void ToggleUniqueDeploy()
    {
        if (_isGasMaskActive)
        {
            DeactivateGasMask();
        }
        else if (chargesRemaining > 0)
        {
            ActivateGasMask();
        }
    }

    void ActivateGasMask()
    {
        _isGasMaskActive = true;
        gasMaskOverlay.SetActive(true);
        // Additional logic to apply gas mask effects, such as reducing damage from gas zones
    }

    void DeactivateGasMask()
    {
        _isGasMaskActive = false;
        gasMaskOverlay.SetActive(false);
        // Additional logic to remove gas mask effects
    }

    public bool IsGasMaskActive()
    {
        return _isGasMaskActive;
    }

    // Update is called once per frame
    void Update()
    {
        ChargesText.text = chargesRemaining.ToString();
    }
}
