using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenPlanetMenu : MonoBehaviour
{
    public string PlanetScene;

    public void onbuttonpress()
    {
        PlayerPrefs.SetInt("Player&CameraDisable", 1);
        SceneManager.LoadSceneAsync(PlanetScene);
    }
}
