using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class FTPpausescript : MonoBehaviour
{
    public GameObject pauseScreen;
    public bool inPauseScreen;
    public InputActionAsset UIasset;
    public InputActionAsset UIassetstage;
    private InputAction UIgoback;
    private InputAction UIpauseopen;
    private InputAction UImenuleft, UImenuright;

    public GameObject stageselect, pausemenumain, optionsscreen, pausemenubackground;
    public GameObject pauseingame, stagescreen1, stagescreen2;

    // Start is called before the first frame update
    void Start()
    {
        UIgoback = UIasset.FindActionMap("UI").FindAction("Cancel");
        UIpauseopen = UIasset.FindActionMap("UI").FindAction("MiddleClick");
        UImenuleft = UIassetstage.FindActionMap("UI").FindAction("Click");
        UImenuright = UIassetstage.FindActionMap("UI").FindAction("RightClick");
        Cursor.lockState = CursorLockMode.Confined;
        inPauseScreen = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (UIpauseopen.WasPressedThisFrame())
        {
            if (pauseingame.activeSelf)
            {
                pauseingame.SetActive(false);
                pausemenubackground.SetActive(true);
                pausemenumain.SetActive(true);
                inPauseScreen = true;
            }
            else
            {
                optionsscreen.SetActive(false);
                stageselect.SetActive(false);
                pausemenumain.SetActive(false);
                pausemenubackground.SetActive(false);
                pauseingame.SetActive(true);
                inPauseScreen = false;
            }
        }

        if (UIgoback.WasPressedThisFrame())
        {
            if (pausemenumain.activeSelf)
            {
                continueButton();
            }
            else if (stageselect.activeSelf)
            {
                stagescreen2.SetActive(false);
                stagescreen1.SetActive(true);
                stageselect.SetActive(false);
                pausemenumain.SetActive(true);
            }
            else if (optionsscreen.activeSelf)
            {
                optionsscreen.SetActive(false);
                pausemenumain.SetActive(true);
            }
        }

        if (stageselect.activeSelf)
        {
            if (stagescreen1.activeSelf)
            {
                if (UImenuleft.WasPressedThisFrame())
                {
                    stagescreen1.SetActive(false);
                    stagescreen2.SetActive(true);
                }
                else if (UImenuright.WasPressedThisFrame())
                {
                    stagescreen1.SetActive(false);
                    stagescreen2.SetActive(true);
                }
            }
            else if (stagescreen2.activeSelf)
            {
                if (UImenuleft.WasPressedThisFrame())
                {
                    stagescreen2.SetActive(false);
                    stagescreen1.SetActive(true);
                }
                else if (UImenuright.WasPressedThisFrame())
                {
                    stagescreen2.SetActive(false);
                    stagescreen1.SetActive(true);
                }
            }
        }
    }

    public void continueButton()
    {
        pausemenumain.SetActive(false);
        pausemenubackground.SetActive(false);
        pauseingame.SetActive(true);
        inPauseScreen = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void loadfirstpersonscene()
    {
        SceneManager.LoadScene("FPtest-SCENE");
    }

    public void loadthirdpersonscene()
    {
        SceneManager.LoadScene("TPtest-SCENE");
    }

    public void loadthirdboostscene()
    {
        SceneManager.LoadScene("TP_BOOSTtest-SCENE");
    }

    public void loadavatartest()
    {
        SceneManager.LoadScene("AVATARtest");
    }

    public void openstageselect()
    {
        pausemenumain.SetActive(false);
        stageselect.SetActive(true);
    }

    public void closestageselect()
    {
        stageselect.SetActive(false);
        pausemenumain.SetActive(true);
    }
    public void openoptionsscreen()
    {
        pausemenumain.SetActive(false);
        optionsscreen.SetActive(true);
    }

    public void closeoptionsscreen()
    {
        optionsscreen.SetActive(false);
        pausemenumain.SetActive(true);
    }

    public void quittomenu()
    {
        SceneManager.LoadScene("mainmenu");
    }

    public void quittodesktop()
    {
        Application.Quit();
    }
}
