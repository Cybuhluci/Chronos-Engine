using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;

    public GameObject stageLoadingPrefab;
    public GameObject miscLoadingPrefab;
    public GameObject databankLoadingPrefab;

    public Scene loadingscene;

    public GameObject currentLoadingScreen;

    public bool isStageLoading;

    public string NextStage;
    public string[] StageNameArray;

    public void StartLoading(string sceneToLoad, bool stageLoading)
    {
        isStageLoading = stageLoading;
        PlayerPrefs.SetString("NextScene", sceneToLoad);
        PlayerPrefs.SetInt("inloadingscreen", 1);
    }

    public void Awake()
    {
        PlayerPrefs.SetInt("inloadingscreen", 1);
        OnLoadingScreenLoaded();
        NextStage = PlayerPrefs.GetString("NextScene", NextStage);
        if (PlayerPrefs.GetInt("IsStageLoading") == 0)
        {
           StartLoading(NextStage, true);
        }
        else
        {
            StartLoading(NextStage, false);
        }

    }

    public void OnLoadingScreenLoaded()
    {
        // Read the loading type: 0 = Misc, 1 = Stage, 2 = Databank
        int loadingType = PlayerPrefs.GetInt("IsStageLoading", 0); // Default to 0 (misc)

        if (currentLoadingScreen != null)
        {
            Destroy(currentLoadingScreen); // Ensure we clear the previous one
        }

        if (loadingType == 1)
        {
            currentLoadingScreen = Instantiate(stageLoadingPrefab);
            Debug.Log("Stage loading screen activated.");
        }
        else if (loadingType == 2)
        {
            currentLoadingScreen = Instantiate(databankLoadingPrefab);
            Debug.Log("Databank loading screen activated.");
        }
        else // Default to Misc
        {
            currentLoadingScreen = Instantiate(miscLoadingPrefab);
            Debug.Log("Misc loading screen activated.");
        }
    }
}
