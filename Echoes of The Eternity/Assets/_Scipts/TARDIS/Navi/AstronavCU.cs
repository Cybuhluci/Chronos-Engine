using UnityEngine;
using TARDIS.Core;

public class AstronavCU : ConsoleCore
{
    // --- TARDISSubsystemController Implementations ---

    // This method is called by the base ToggleCircuit() when _isCircuitActive becomes TRUE.
    protected override void OnCircuitActivated() { }
    // This method is called by the base ToggleCircuit() when _isCircuitActive becomes FALSE.
    protected override void OnCircuitDeactivated() { }

    [SerializeField] NaviCore deltaCircuit;
    [SerializeField] GameObject astronavUI;
    [SerializeField] FirstPersonController playerController;

    private void Awake()
    {
        ToggleCircuit();
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
