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
    }

    public void CloseAstronav()
    {
        astronavUI.SetActive(false);
    }
}
