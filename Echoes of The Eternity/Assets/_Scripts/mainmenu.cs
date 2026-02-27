using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class mainmenu : MonoBehaviour
{
    public InputActionAsset UIasset;
    private InputAction UIgoback;
    private InputAction UItitleanykey;
    public GameObject wholeofthemenu;

    public GameObject creditsScreen, mainmenuscreen, optionsscreen;
    public GameObject titlescreen;

    // Stage Selection Screens
    public GameObject[] stageScreens;
    private int currentStageIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        UIgoback = UIasset.FindAction("Cancel");
        UItitleanykey = UIasset.FindAction("AnyKey");

        UIgoback.Enable();
        UItitleanykey.Enable();

        Cursor.lockState = CursorLockMode.Confined;
    }

    // Update is called once per frame
    void Update()
    {
        if (UIgoback.WasPressedThisFrame())
        {
            if (mainmenuscreen.activeSelf)
            {
                optionsscreen.SetActive(false);
                mainmenuscreen.SetActive(true);
                wholeofthemenu.SetActive(false);
                titlescreen.SetActive(true);
            }
            else if (optionsscreen.activeSelf)
            {
                optionsscreen.SetActive(false);
                mainmenuscreen.SetActive(true);
            }
            else if (creditsScreen.activeSelf)
            {
                creditsScreen.SetActive(false);
                mainmenuscreen.SetActive(true);
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
    }

    public void playgame()
    {
        StageManager.Instance.LoadMiscScene("Test");
    }

    public void opencreditsscreen()
    {
        mainmenuscreen.SetActive(false);
        creditsScreen.SetActive(true);
    }

    public void openoptionsscreen()
    {
        mainmenuscreen.SetActive(false);
        optionsscreen.SetActive(true);
    }

    public void quittodesktop()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Only works in editor
#endif
    }
}
