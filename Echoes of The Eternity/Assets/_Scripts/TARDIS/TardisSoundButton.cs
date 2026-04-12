using UnityEngine;

public class TardisSoundButton : MonoBehaviour
{
    public static TardisSoundButton Instance { get; private set; }

    [SerializeField] private AudioClip[] buttonSounds;
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void PlayButtonSoundInObject() // plays sounds stored within buttonSounds.
    {
        audioSource.PlayOneShot(buttonSounds[Random.Range(0, buttonSounds.Length)]);
    }

    public void PlayButtonSoundFromElsewhere(AudioClip soundbyte)
    {
        audioSource.PlayOneShot(soundbyte);
    }
}
