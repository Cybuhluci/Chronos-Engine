using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class mainmenu : MonoBehaviour
{
    public InputActionAsset UIasset;
    public InputActionAsset UIassetstage;
    public InputActionAsset UIAnyKey;
    private InputAction UIgoback;
    private InputAction UItitleanykey;
    private InputAction UImenuleft, UImenuright;
    public GameObject wholeofthemenu;

    public GameObject stageselect, mainmenuscreen, optionsscreen, quitmenu, codexscreen, fileselect, playmenu;
    public GameObject titlescreen;

    // Stage Selection Screens
    public GameObject[] stageScreens;
    private int currentStageIndex = 0;

    public AudioMixer theMixer;

    // Start is called before the first frame update
    void Start()
    {
        UIgoback = UIasset.FindActionMap("UI").FindAction("Cancel");
        UItitleanykey = UIAnyKey.FindActionMap("UI").FindAction("anyandall");
        UImenuleft = UIassetstage.FindActionMap("UI").FindAction("Click");
        UImenuright = UIassetstage.FindActionMap("UI").FindAction("RightClick");
        Cursor.lockState = CursorLockMode.Confined;

        // Load saved audio settings
        if (PlayerPrefs.HasKey("Master"))
        {
            theMixer.SetFloat("Master", PlayerPrefs.GetFloat("Master"));
        }
        if (PlayerPrefs.HasKey("MUSIC"))
        {
            theMixer.SetFloat("MUSIC", PlayerPrefs.GetFloat("MUSIC"));
        }
        if (PlayerPrefs.HasKey("SFX"))
        {
            theMixer.SetFloat("SFX", PlayerPrefs.GetFloat("SFX"));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (UIgoback.WasPressedThisFrame())
        {
            if (mainmenuscreen.activeSelf)
            {
                optionsscreen.SetActive(false);
                stageselect.SetActive(false);
                mainmenuscreen.SetActive(true);
                wholeofthemenu.SetActive(false);
                titlescreen.SetActive(true);
            }
            else if (stageselect.activeSelf)
            {
                stageselect.SetActive(false);
                mainmenuscreen.SetActive(true);
            }
            else if (optionsscreen.activeSelf)
            {
                optionsscreen.SetActive(false);
                mainmenuscreen.SetActive(true);
            }
            else if (codexscreen.activeSelf)
            {
                codexscreen.SetActive(false);
                mainmenuscreen.SetActive(true);
            }
            else if (playmenu.activeSelf)
            {
                playmenu.SetActive(false);
                mainmenuscreen.SetActive(true);
            }
            else if (fileselect.activeSelf)
            {
                fileselect.SetActive(false);
                playmenu.SetActive(true);
            }
        }
        
        if (UItitleanykey.WasPressedThisFrame())
        {
            if (titlescreen.activeSelf)
            {
                titlescreen.SetActive(false);
                wholeofthemenu.SetActive(true);
            }
        }

        if (stageselect.activeSelf)
        {
            if (UImenuleft.WasPressedThisFrame())
            {
                ChangeStageScreen(-1);
            }
            else if (UImenuright.WasPressedThisFrame())
            {
                ChangeStageScreen(1);
            }
        }
    }

    void ChangeStageScreen(int direction)
    {
        stageScreens[currentStageIndex].SetActive(false);
        currentStageIndex = (currentStageIndex + direction + stageScreens.Length) % stageScreens.Length;
        stageScreens[currentStageIndex].SetActive(true);
    }

    public void continuegame()
    {
        SaveManager.LoadGame();
        string continueloc = PlayerPrefs.GetString("NextScene");

        if (continueloc != null || continueloc == "")
        {
            LoadStage(continueloc);
        }
        else
        {
            newgame();
        }
    }

    public void loadgame()
    {
        SaveManager.LoadGame();

        if (SaveManager.HasCollected("Tardis Key"))
        {
            LoadStage("TARDISinterior");
        }
        else
        {
            newgame();
        }
    }

    public void newgame()
    {
        SaveManager.ResetGame();
        LoadMiscScene("pilotmission");
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
    
    public void LoadDatabankScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("ERROR: Scene name is empty or null!");
            return;
        }

        // Save scene names
        PlayerPrefs.SetString("NextScene", sceneName);
        PlayerPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.SetInt("IsStageLoading", 2); // 0 = Misc
        PlayerPrefs.Save(); // Ensure data is written

        Debug.Log($"Loading screen opened. Next Scene: {sceneName}, Previous Scene: {SceneManager.GetActiveScene().name}");

        SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);
    }

    public void openstageselect()
    {
        mainmenuscreen.SetActive(false);
        stageselect.SetActive(true);
    }

    public void openoptionsscreen()
    {
        mainmenuscreen.SetActive(false);
        optionsscreen.SetActive(true);
    }

    public void closeoptionsscreen()
    {
        optionsscreen.SetActive(false);
        mainmenuscreen.SetActive(true);
    }

    public void quittomenu()
    {
        SceneManager.LoadScene("mainmenu");
    }

    public void openquitmenu()
    {
        quitmenu.SetActive(true);
        mainmenuscreen.SetActive(false);
    }

    public void clostquitmenu()
    {
        quitmenu.SetActive(false);
        mainmenuscreen.SetActive(true);
    }

    public void quittodesktop()
    {
        Application.Quit();
    }

    public void opencodexmenu()
    {
        mainmenuscreen.SetActive(false);
        codexscreen.SetActive(true);
    }

    public void openplaymenu()
    {
        mainmenuscreen.SetActive(false);
        playmenu.SetActive(true);
    }

    public void openfileselect()
    {
        playmenu.SetActive(false);
        fileselect.SetActive(true);
    }
}
