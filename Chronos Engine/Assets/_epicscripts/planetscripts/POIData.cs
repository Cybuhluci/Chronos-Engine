using UnityEngine;

[CreateAssetMenu(fileName = "NewPOI", menuName = "Systems/POI Data")]
public class POIData : ScriptableObject
{
    [Header("Hover-over Menu")]
    public Sprite poiHoverIcon;
    public string poiShortName; // Name of the POI
    public string poiShortDescription; // Short description

    [Header("Stages")] // only goes up to 6 places per area, since i am not adding scrolling for actually infinite.
    public bool[] stagesAvailable = new bool[6];  // True/False for each stage, flexible for 6 stages or more.

    [Header("Stage Select Prefab")] // ill keep this here until im sure the stage select shouldnt be instantiated.
    public GameObject poiStageSelectMenu; // prefab to use for the menu, event system and ui input inside
}