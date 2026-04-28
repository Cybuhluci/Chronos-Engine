using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenBakcground : MonoBehaviour
{
    public Sprite[] BackgroundImages; // Array of background images to cycle through
    public Image BackgroundImage;
    public float cycleInterval = 5f; // Time in seconds between background changes

    private int currentIndex = 0;
    private float timer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (BackgroundImages.Length > 0 && BackgroundImage != null)
        {
            BackgroundImage.sprite = BackgroundImages[currentIndex]; // Set initial background
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (BackgroundImages.Length > 0 && BackgroundImage != null)
        {
            timer += Time.deltaTime;
            if (timer >= cycleInterval)
            {
                timer = 0f;
                currentIndex = (currentIndex + 1) % BackgroundImages.Length;
                BackgroundImage.sprite = BackgroundImages[currentIndex];
            }
        }
    }
}
