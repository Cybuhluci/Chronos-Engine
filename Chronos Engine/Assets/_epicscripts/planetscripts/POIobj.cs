using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class POIobj : MonoBehaviour
{
    public POIData poiData; // Reference to the ScriptableObject

    [Header("Stage Select")]
    public Image poiIconUI; // UI Element to display the main icon
    public GameObject StageSelect;
    public GameObject StageSelectDefault;

    [Header("Stage 1")]
    public Image poiStageImage1;
    public TMP_Text poiDescription1;

    [Header("Stage 2")]
    public Image poiStageImage2;
    public TMP_Text poiDescription2;

    [Header("Stage 3")]
    public Image  poiStageImage3; // Images for locations
    public TMP_Text poiDescription3; // Descriptions for locations

    [Header("Hover-over Menu")]
    public Image poiHoverIconUI;
    public TMP_Text poiShortNameUI;
    public TMP_Text poiShortDescriptionUI;

    public void DisplayHoverInfo()
    {
        if (poiData == null) return;

        poiHoverIconUI.sprite = poiData.poiHoverIcon;
        poiShortNameUI.text = poiData.poiShortName;
        poiShortDescriptionUI.text = poiData.poiShortDescription;
    }

    public void DisplayStageInfo()
    {

    }
}
