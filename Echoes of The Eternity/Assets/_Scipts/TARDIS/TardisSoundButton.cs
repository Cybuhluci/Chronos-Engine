using UnityEngine;

public class TardisSoundButton : MonoBehaviour
{
    [SerializeField] private string buttonName;
    [SerializeField] private AudioClip[] buttonSounds;
    [SerializeField] private AudioSource audioSource;

    public void PlayButtonSound()
    {
        audioSource.PlayOneShot(buttonSounds[Random.Range(0, buttonSounds.Length)]);
    }
}
