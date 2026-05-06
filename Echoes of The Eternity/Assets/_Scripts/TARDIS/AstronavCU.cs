using Luci;
using UnityEngine;
using UnityEngine.InputSystem;

public class AstronavCU : MonoBehaviour
{
    [SerializeField] GameObject astronavUI;
    [SerializeField] PlayerInput playerInput;

    private void Update()
    {
        if (playerInput.actions["Cancel"].WasPressedThisFrame() && astronavUI.activeSelf)
        {
            CloseAstronav();
        }
    }

    public void OpenAstronav()
    {
        astronavUI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        FirstPersonController.Instance.playerHUD.SetActive(false);
        FirstPersonController.Instance.ToggleDisableCamera(true);
        FirstPersonController.Instance.ToggleDisableMovement(true);
    }

    public void CloseAstronav()
    {
        astronavUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        FirstPersonController.Instance.playerHUD.SetActive(true);
        FirstPersonController.Instance.ToggleDisableCamera(false);
        FirstPersonController.Instance.ToggleDisableMovement(false);
    }
}
