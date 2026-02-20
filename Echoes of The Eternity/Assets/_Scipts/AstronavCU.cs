using UnityEngine;

public class AstronavCU : MonoBehaviour
{
    [SerializeField] GameObject astronavUI;
    [SerializeField] FirstPersonController playerController;

    private void Awake()
    {

    }

    private void Update()
    {
        if (playerController.playerinput.actions["Tab"].WasPressedThisFrame() && astronavUI.activeSelf)
        {
            playerController.SetControl(true);
            astronavUI.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void OpenAstronav()
    {
        playerController.SetControl(false);
        astronavUI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
