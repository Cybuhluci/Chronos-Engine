using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadManager : MonoBehaviour
{
    public int debugmode = 0;

    private void Start()
    {
        if (debugmode == 0)
        {
            SaveManager.LoadGame();
        }
        else
        {
            SaveManager.ResetGame();
            SaveManager.LoadGame();
            PlayerPrefs.SetInt("Player&CameraDisable", 0);
            PlayerPrefs.SetInt("PlayerDisable", 0);
            PlayerPrefs.SetInt("CameraDisable", 0);
        }
    }
}
