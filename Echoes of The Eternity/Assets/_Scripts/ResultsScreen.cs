using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ResultsScreen : MonoBehaviour
{
    [SerializeField] private GameObject normalHUD;
    public bool inResultsScreen = false;

    [SerializeField] private GameObject resultsScreenUI;
    [SerializeField] private Image EXPbar;
    [SerializeField] private TMP_Text EXPText;
    [SerializeField] private TMP_Text playerText;
    float EXPtoAdd;
    float EXPtoLevelUp = 10000;
    float currentEXP;
    float animAccumulated = 0f;
    float bagsToAdd;
    int playerLevel;

    private LootVanScript lootVan;

    [Header("Results UI")]
    [SerializeField] private TMP_Text summaryBox;

    private void Start()
    {
        lootVan = FindFirstObjectByType<LootVanScript>();
    }

    public void StartResults()
    {
        inResultsScreen = true;
        normalHUD.SetActive(false);

        EXPtoAdd = lootVan.GetStoredEXP();
        bagsToAdd = lootVan.GetStoredBags();

        resultsScreenUI.SetActive(true);

        Cursor.lockState = CursorLockMode.None;

        SummariseMoneyGain();
    }

    void SummariseMoneyGain()
    {
        summaryBox.text = $"You gained {bagsToAdd} bags and {EXPtoAdd} EXP!";

        VisualiseEXPGain();
    }

    void VisualiseEXPGain()
    {
        // prepare animation state
        currentEXP = 0f;
        animAccumulated = 0f;

        // ensure EXP bar is using Filled type so fillAmount is visible
        if (EXPbar != null)
            EXPbar.type = Image.Type.Filled;

        StartCoroutine(AnimateEXPGain());
    }

    private IEnumerator AnimateEXPGain()
    {
        // We'll animate accumulated EXP and update the bar relative to level thresholds
        float speedMultiplier = 1f; // tweak if you want faster/slower animation
        while (animAccumulated < EXPtoAdd)
        {
            float delta = EXPtoAdd * Time.deltaTime * speedMultiplier;
            animAccumulated += delta;
            if (animAccumulated > EXPtoAdd) animAccumulated = EXPtoAdd;

            // total EXP applied to player = animAccumulated
            // compute how many levels that crosses and the progress within current level
            int levelsGained = Mathf.FloorToInt(animAccumulated / EXPtoLevelUp);
            float progressInLevel = animAccumulated - levelsGained * EXPtoLevelUp;

            // update UI
            if (EXPbar != null)
                EXPbar.fillAmount = Mathf.Clamp01(progressInLevel / EXPtoLevelUp);

            if (EXPText != null)
                EXPText.text = $"{(int)progressInLevel} / {(int)EXPtoLevelUp} EXP";

            if (playerText != null)
                playerText.text = $"{playerLevel + levelsGained}";

            yield return null;
        }

        // finalize: apply all gained levels to playerLevel and set bar to final state
        int finalLevels = Mathf.FloorToInt(animAccumulated / EXPtoLevelUp);
        float finalProgress = animAccumulated - finalLevels * EXPtoLevelUp;
        playerLevel += finalLevels;
        if (EXPbar != null) EXPbar.fillAmount = Mathf.Clamp01(finalProgress / EXPtoLevelUp);
        if (EXPText != null) EXPText.text = $"{(int)finalProgress} / {(int)EXPtoLevelUp} EXP";
        if (playerText != null) playerText.text = $"{playerLevel}";

        //while (currentEXP < EXPtoAdd)
        //{
        //    currentEXP += EXPtoAdd * Time.deltaTime; // Adjust the speed of the animation by changing the multiplier
        //    if (currentEXP > EXPtoAdd) currentEXP = EXPtoAdd; // Ensure we don't exceed the target EXP
        //    // Update the UI elements
        //    EXPbar.fillAmount = currentEXP / EXPtoAdd;
        //    EXPText.text = $"{(int)currentEXP} / {(int)EXPtoAdd} EXP";
        //    yield return null; // Wait for the next frame
        //}
    }
}
