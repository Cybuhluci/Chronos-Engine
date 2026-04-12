using UnityEngine;

public class Handbrake : MonoBehaviour
{
    public bool isActive = true;

    [SerializeField] private AudioClip _engageSound, _disengageSound;

    public void PressInteract()
    {
        ToggleHandbrake();
    }

    private void ToggleHandbrake()
    {
        isActive = !isActive;
        if (isActive)
        {
            TardisSoundButton.Instance.PlayButtonSoundFromElsewhere(_engageSound);
        }
        else
        {
            TardisSoundButton.Instance.PlayButtonSoundFromElsewhere(_disengageSound);
        }
    }
}