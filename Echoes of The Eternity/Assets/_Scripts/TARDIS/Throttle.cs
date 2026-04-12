using UnityEngine;

public class Throttle : MonoBehaviour
{
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
        TardisSoundButton.Instance.PlayButtonSoundFromElsewhere(_notificationSound);
        RUSureHUD.SetActive(true);
    }

    public void EngageThrottle()
    {
        // begin heist async loading sequence
        // lock the front door while in flight.
        // unlock the front door when the heist is loaded and the player can exit the TARDIS

        StageManager.Instance.LoadStage(_MissionSelect.GetStoredMission());
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
    }

    public void NoDestinationSetOpen()
    {
        TardisSoundButton.Instance.PlayButtonSoundFromElsewhere(_notificationSound);
        NoDestinationSetHUD.SetActive(true);
    }

    public void NoDestinationSetClose()
    {

        CancelTakeoff();
        NoDestinationSetHUD.SetActive(false);
        CancelTakeoff();
    }
}