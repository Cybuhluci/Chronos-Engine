using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class creditsTextScroll : MonoBehaviour
{
    [Header("Main Elements")]
    public Transform TheEnd; // "The End" text/image
    public Transform GameTitle; // Final title of the game
    public Transform SpawnPoint; // Where credits spawn
    public GameObject CreditPrefab; // Prefab for credit text

    [Header("Credit Settings")]
    private string[] CreditText;
    [Header("External Credits File")]
    [Tooltip("Optional: TextAsset containing credits, one entry per line. If set, this will override the built-in list.")]
    public TextAsset creditsTextAsset;
    public float spacing = 50f; // Spacing between credit lines
    public float endMoveSpeed = 50f; // Speed at which "The End" moves up
    public float creditsScrollSpeed = 50f; // Speed of credits scrolling
    public float endPauseDuration = 2f; // How long "The End" stays in place
    public float logoMoveSpeed = 30f; // Speed for the game title rising

    public bool creditsStarted = false;
    public GameObject[] spawnedCredits; // To store instantiated credits
    public AudioSource creditsMusic; // Music to play during credits

    void Start()
    {
        GameTitle.gameObject.SetActive(false); // Hide the game title at start

        // If an external credits text asset is provided, parse it into the CreditText array
        if (creditsTextAsset != null)
        {
            try
            {
                var lines = creditsTextAsset.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 0)
                {
                    CreditText = lines;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to parse creditsTextAsset: {e.Message}");
            }
        }

        creditsMusic.Play(); // Start playing the credits music
        StartCoroutine(ShowTheEnd());
    }

    IEnumerator ShowTheEnd()
    {
        // Move "The End" upwards until it's centered
        while (TheEnd.position.y < Screen.height * 0.5f)
        {
            TheEnd.position += Vector3.up * endMoveSpeed * Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(endPauseDuration); // Pause dramatically
        StartCoroutine(SpawnCredits()); // Start credits as "The End" moves up

        // Now continue moving it offscreen
        while (TheEnd.position.y < Screen.height * 1.2f) // Move past top edge
        {
            TheEnd.position += Vector3.up * endMoveSpeed * Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator SpawnCredits()
    {
        creditsStarted = true;
        spawnedCredits = new GameObject[CreditText.Length];

        // Instantiate all credit lines with proper spacing
        for (int i = 0; i < CreditText.Length; i++)
        {
            GameObject credit = Instantiate(CreditPrefab, SpawnPoint);
            credit.GetComponent<TMP_Text>().text = CreditText[i];
            credit.transform.localPosition = new Vector3(0, -spacing * i, 0); // Staggered spacing
            spawnedCredits[i] = credit;
            yield return null; // Just to keep things smooth
        }

        StartCoroutine(ScrollCredits());
    }

    IEnumerator ScrollCredits()
    {
        bool lastCreditReachedMiddle = false;
        bool allCreditsOffscreen = false;

        while (!allCreditsOffscreen)
        {
            allCreditsOffscreen = true;

            for (int i = 0; i < spawnedCredits.Length; i++)
            {
                GameObject credit = spawnedCredits[i];

                if (credit != null)
                {
                    credit.transform.position += Vector3.up * creditsScrollSpeed * Time.deltaTime;

                    // Check when the LAST credit reaches the middle of the screen
                    if (!lastCreditReachedMiddle && i == spawnedCredits.Length - 1 &&
                        credit.transform.position.y >= Screen.height * 0.5f)
                    {
                        lastCreditReachedMiddle = true;
                        StartCoroutine(ShowGameTitle()); // Start moving the logo up
                    }

                    // If any credit is still on-screen, keep scrolling
                    if (credit.transform.position.y < Screen.height * 1.2f)
                    {
                        allCreditsOffscreen = false;
                    }
                }
            }

            yield return null;
        }
    }


    IEnumerator ShowGameTitle()
    {
        GameTitle.gameObject.SetActive(true); // Activate the logo
        while (GameTitle.position.y < Screen.height * 0.5f)
        {
            GameTitle.position += Vector3.up * logoMoveSpeed * Time.deltaTime;
            yield return null;
        }
    }
}