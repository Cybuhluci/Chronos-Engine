using UnityEngine;

public class planetaudio : MonoBehaviour
{
    public AudioSource AudioSource;
    public AudioClip MenuMusic;

    // Start is called before the first frame update
    void Start()
    {
        AudioSource = GetComponent<AudioSource>();
        AudioSource.PlayOneShot(MenuMusic);
    }
}
