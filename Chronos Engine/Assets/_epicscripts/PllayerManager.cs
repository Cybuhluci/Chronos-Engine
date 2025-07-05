using Unity.Cinemachine;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private TPmovement Movement;
    [SerializeField] private GameObject PlayerModelObject;
    [SerializeField] private GameObject PlayerFullObject;
    [SerializeField] private CinemachineBrain Brain;

    private void Update()
    {
        // Get PlayerPrefs flags only once
        bool fullDisable = PlayerPrefs.GetInt("Player&CameraDisable", 0) == 1;
        bool playerDisable = PlayerPrefs.GetInt("PlayerDisable", 0) == 1;
        bool cameraDisable = PlayerPrefs.GetInt("CameraDisable", 0) == 1;

        // Full disable overrides others
        if (fullDisable)
        {
            SetPlayerVisible(false);
            SetCameraActive(false);
            return;
        }

        // Apply individual flags
        SetPlayerVisible(!playerDisable);
        SetCameraActive(!cameraDisable);
    }

    private void SetPlayerVisible(bool visible)
    {
        if (PlayerModelObject != null)
            PlayerModelObject.SetActive(visible);
    }

    private void SetCameraActive(bool active)
    {
        if (Brain != null)
            Brain.enabled = active;
    }
}

// NOT SURE IF I STILL WANT "DONTDESTROYONLOAD" SINCE ITS NOW EPISODE BASED AND NOT FREE ROAM
// therefore TARDIS is now seperate sceen used between levels with a location outside which IS a free roam planet.
//if (instance == null)
//{
//    instance = this;
//    DontDestroyOnLoad(gameObject);
//}
//else
//{
//    Destroy(gameObject); // Prevent duplicates if the script is on multiple objects
//}