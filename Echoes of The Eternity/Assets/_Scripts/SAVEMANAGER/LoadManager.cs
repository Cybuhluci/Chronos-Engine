using UnityEngine;

namespace Luci.Saving
{
    public class LoadManager : MonoBehaviour
    {
        public static LoadManager Instance { get; private set; }

        public SaveManager saveManager;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        public int debugmode = 0;

        private void Start()
        {
            if (debugmode == 0)
            {
                saveManager.LoadGame();
            }
            else if (debugmode == 1)
            {
                saveManager.ResetGame();
                saveManager.LoadGame();
                PlayerPrefs.SetInt("Player&CameraDisable", 0);
                PlayerPrefs.SetInt("PlayerDisable", 0);
                PlayerPrefs.SetInt("CameraDisable", 0);
            }
            else if (debugmode == 2)
            {
                saveManager.NewGame();
            }
            else
            {
                // do nothing, for testing purposes
            }
        }
    }
}