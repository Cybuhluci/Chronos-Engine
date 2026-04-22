using Luci;
using UnityEngine;
using UnityEngine.InputSystem;

public class FTPpausescript : MonoBehaviour
{
    public FirstPersonController playerController;

    public bool inPauseScreen;

    public PlayerInput UIasset;
    private InputAction UIgoback;

    public GameObject playerHUD;
    public GameObject pausemenumain;
    public GameObject optionsScreen;

    void Start()
    {
        UIgoback = UIasset.actions["Cancel"];
    }

    void Update()
    {
        if (UIgoback.WasPerformedThisFrame())
        {
            HandleBack();
        }
    }

    public void TogglePause()
    {
        optionsScreen.SetActive(false);

        if (inPauseScreen == false)
        {
            Time.timeScale = 0f;
            playerController.ToggleDisableCamera(true);
            playerController.ToggleDisableMovement(true);
            pausemenumain.SetActive(true);
            playerHUD.SetActive(false);
            inPauseScreen = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            Time.timeScale = 1f;
            playerController.ToggleDisableCamera(false);
            playerController.ToggleDisableMovement(false);
            pausemenumain.SetActive(false);
            playerHUD.SetActive(true);
            inPauseScreen = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void HandleBack()
    {
        if (optionsScreen.activeSelf)
        {
            ToggleOptions(false);
        }
    }

    public void continueButton()
    {
        TogglePause();
    }

    public void returnToMainMenu()
    {
        Time.timeScale = 1f;
        StageManager.Instance.LoadMiscScene("mainmenu");
    }

    public void returnToDesktop()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void ToggleOptions(bool active)
    {
        optionsScreen.SetActive(active);
    }
}
