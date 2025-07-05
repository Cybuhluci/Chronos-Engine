using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    private List<string> loadedScenes = new List<string>();
    public string currentScene;

    public void LoadMiscScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("ERROR: Scene name is empty or null!");
            return;
        }

        // Save scene names
        PlayerPrefs.SetString("NextScene", sceneName);
        PlayerPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.SetInt("IsStageLoading", 0); // 0 = Misc
        PlayerPrefs.Save(); // Ensure data is written

        Debug.Log($"Loading screen opened. Next Scene: {sceneName}, Previous Scene: {SceneManager.GetActiveScene().name}");

        SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);
    }

    public void LoadStage(string stageName)
    {
        if (string.IsNullOrEmpty(stageName))
        {
            Debug.LogError("ERROR: Scene name is empty or null!");
            return;
        }

        // Save scene names
        PlayerPrefs.SetString("NextScene", stageName);
        PlayerPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.SetInt("IsStageLoading", 1); // 1 = Stage
        PlayerPrefs.Save(); // Ensure data is written

        Debug.Log($"Loading screen opened. Next Scene: {stageName}, Previous Scene: {SceneManager.GetActiveScene().name}");

        SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);
    }

    public void LoadTARDISScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("ERROR: Scene name is empty or null!");
            return;
        }

        // Save scene names
        PlayerPrefs.SetString("NextScene", sceneName);
        PlayerPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.SetInt("IsStageLoading", 2); // 2 = TARDIS int/ext
        PlayerPrefs.Save(); // Ensure data is written

        Debug.Log($"Loading screen opened. Next Scene: {sceneName}, Previous Scene: {SceneManager.GetActiveScene().name}");

        SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);
    }
}
