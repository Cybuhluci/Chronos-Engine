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

    public override void RefillUDMeter(float percentage)
    {
        // example implementation: increase a meter value (0..100). When meter reaches 100, convert to a chunk
        // This class currently tracks chargesRemaining, so we'll use a simple meter internal to refill and add chunks.
        // For now, implement a simple behaviour: add percentage to an internal meter, when >=100 add 1 charge and subtract 100.
        meterProgress += percentage;
        while (meterProgress >= 100f)
        {
            meterProgress -= 100f;
            chargesRemaining = Mathf.Min(chargesRemaining + 1, gasMaskData.maxChunks);
        }
    }

    private float meterProgress = 0f;

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

    public void RefillUD(float percentage)
    {
        // fills the chunkImage, when the image is full, adds 1 ammo.
        if (gasMaskCooldown != null)
        {
            if (chargesRemaining >= gasMaskData.maxChunks) return; // Don't fill if already at max charges
            gasMaskCooldown.fillAmount += percentage;
            if (gasMaskCooldown.fillAmount >= 1f)
            {
                gasMaskCooldown.fillAmount = 0f;
                chargesRemaining++;
            }
        }
    }
}
