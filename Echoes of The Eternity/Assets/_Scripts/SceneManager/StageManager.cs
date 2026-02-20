using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

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

    public void LoadSceneDeveloper(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("ERROR: Scene name is empty or null!");
            return;
        }

        // Save scene names
        PlayerPrefs.SetString("NextScene", sceneName);
        PlayerPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.SetInt("IsStageLoading", 2); // 2 = Developer
        PlayerPrefs.Save(); // Ensure data is written

        Debug.Log($"Loading screen opened. Next Scene: {sceneName}, Previous Scene: {SceneManager.GetActiveScene().name}");

        SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);
    }

    public void ReloadScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Save scene names
        PlayerPrefs.SetString("NextScene", currentScene);
        PlayerPrefs.SetString("PreviousScene", currentScene);
        PlayerPrefs.SetInt("IsStageLoading", 3); // 3 = Reload
        PlayerPrefs.Save(); // Ensure data is written

        Debug.Log($"Reloading current scene: {currentScene}");
        SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);
    }
}
