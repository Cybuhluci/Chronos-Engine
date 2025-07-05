using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageSelectUpdater : MonoBehaviour
{
    public POIInteract interactor;
    public GameObject[] Buttons;

    // Start is called before the first frame update
    void Awake()
    {
        UpdateButtonVisibility();
    }

    void UpdateButtonVisibility()
    {
        // Assuming POIData is attached to the currentPOI
        POIData currentPOIData = interactor.currentPOI.poiData;

        // Loop through the Buttons array and disable buttons based on stages available
        for (int i = 0; i < Buttons.Length; i++)
        {
            // Determine if the stage is available
            bool isStageAvailable = false;

            switch (i)
            {
                case 0:
                    isStageAvailable = currentPOIData.stagesAvailable[0];
                    break;
                case 1:
                    isStageAvailable = currentPOIData.stagesAvailable[1];
                    break;
                case 2:
                    isStageAvailable = currentPOIData.stagesAvailable[2];
                    break;
                case 3:
                    isStageAvailable = currentPOIData.stagesAvailable[3];
                    break;
                case 4:
                    isStageAvailable = currentPOIData.stagesAvailable[4];
                    break;
                case 5:
                    isStageAvailable = currentPOIData.stagesAvailable[5];
                    break;
            }

            // Enable/Disable buttons based on availability
            Buttons[i].SetActive(isStageAvailable);
        }
    }
}
