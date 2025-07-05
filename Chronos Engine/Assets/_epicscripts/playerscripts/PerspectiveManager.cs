using UnityEngine;

public class PerspectiveManager : MonoBehaviour
{
    [SerializeField] private TPmovement movement;

    public void ToggleToFirstPerson()
    {
        if (movement.cameraStyle != TPmovement.camerastyle.first)
        {
            movement.ChangeToFirst();
        }
    }

    public void ToggleToThirdPerson()
    {
        if (movement.cameraStyle != TPmovement.camerastyle.third)
        {
            movement.ChangeToThird();
        }
    }

    public void ToggleToSecondPerson()
    {
        if (movement.cameraStyle != TPmovement.camerastyle.third)
        {
            movement.ChangeToSecond();
        }
    }
}
