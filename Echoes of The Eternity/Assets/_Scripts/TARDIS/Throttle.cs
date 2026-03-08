using UnityEngine;

public class Throttle : MonoBehaviour
{
    [SerializeField] MissionSelect _MissionSelect;

    public void PressInteract()
    {
        ShowRUSureHUD();
    }

    public void ShowRUSureHUD()
    {
        // Show a HUD asking the player if they are sure they want to engage the throttle
        // If they confirm, call EngageThrottle()
        EngageThrottle();
    }

    public void EngageThrottle()
    {
        // begin heist async loading sequence
        // lock the front door while in flight.
        // unlock the front door when the heist is loaded and the player can exit the TARDIS

        StageManager.Instance.LoadStage(_MissionSelect.GetStoredMission());
    }
}