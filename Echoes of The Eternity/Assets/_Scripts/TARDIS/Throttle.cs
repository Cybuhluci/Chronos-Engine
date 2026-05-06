using Luci;
using TARDIS.Main;
using UnityEngine;

public class Throttle : MonoBehaviour
{
    [SerializeField] private _42Main TardisMain;
    [SerializeField] MissionSelect _MissionSelect;
    public Handbrake _Handbrake;
    [SerializeField] private GameObject RUSureHUD, NoDestinationSetHUD;
    [SerializeField] private AudioClip _engageSound, _disengageSound;
    [SerializeField] private AudioClip _notificationSound;

    public void PressInteract()
    {
        if (!_Handbrake.isActive)
        {
            // make notification appear (general notification script to be made, and it will be amazing)
        }
        else
        {
            TardisSoundButton.Instance.PlayButtonSoundFromElsewhere(_engageSound);
            ShowRUSureHUD();
        }
    }

    public void ShowRUSureHUD()
    {
        Cursor.lockState = CursorLockMode.None;
        FirstPersonController.Instance.playerHUD.SetActive(false);
        FirstPersonController.Instance.ToggleDisableCamera(true);
        FirstPersonController.Instance.ToggleDisableMovement(true);
        TardisSoundButton.Instance.PlayButtonSoundFromElsewhere(_notificationSound);
        RUSureHUD.SetActive(true);
    }

    public void EngageThrottle()
    {
        // begin heist async loading sequence
        // lock the front door while in flight.
        // unlock the front door when the heist is loaded and the player can exit the TARDIS
        Cursor.lockState = CursorLockMode.Locked;
        FirstPersonController.Instance.playerHUD.SetActive(true);
        FirstPersonController.Instance.ToggleDisableCamera(false);
        FirstPersonController.Instance.ToggleDisableMovement(false);

        TardisMain.BeginFlightToLocation();
    }

    public void ConfirmTakeoff()
    {
        if (_MissionSelect.GetStoredMission() == null)
        {
            NoDestinationSetOpen();
        }
        else
        {
            RUSureHUD.SetActive(false);
            EngageThrottle();
        }
    }

    public void CancelTakeoff()
    {
        TardisSoundButton.Instance.PlayButtonSoundFromElsewhere(_disengageSound);
        RUSureHUD.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void NoDestinationSetOpen()
    {
        Cursor.lockState = CursorLockMode.None;
        TardisSoundButton.Instance.PlayButtonSoundFromElsewhere(_notificationSound);
        NoDestinationSetHUD.SetActive(true);
        FirstPersonController.Instance.playerHUD.SetActive(false);
        FirstPersonController.Instance.ToggleDisableCamera(true);
        FirstPersonController.Instance.ToggleDisableMovement(true);
    }

    public void NoDestinationSetClose()
    {
        FirstPersonController.Instance.playerHUD.SetActive(true);
        FirstPersonController.Instance.ToggleDisableCamera(false);
        FirstPersonController.Instance.ToggleDisableMovement(false);
        CancelTakeoff();
        NoDestinationSetHUD.SetActive(false);
        CancelTakeoff();
    }
}