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
    public string[] CreditText = { // List of credits to show
    "Time Lord Creator - Luci",
    "The Ultimate Code Architect - Luci",
    "Lead Programmer - Luci",
    "Lead Designer - Luci",
    "Assistant Timekeeper - Luci",
    "Sonic Screwdriver Developer - Luci",
    "Temporal Debugger - Luci",
    "Dimensional Engineer - Luci",
    "Graphics Sorcerer - Luci",
    "The Office Coffee Brewer - Luci",
    "Caffeine Manager - Luci",
    "Official Tea Maker - Luci",
    "TARDIS Console Maintainer - Luci",
    "Cosmic Bug Exterminator - Luci",
    "Temporal Consultant - Luci",
    "Lead Galactic Architect - Luci",
    "Interdimensional Assistant - Luci",
    "Time-Space Coordinator - Luci",
    "Sofa Placer - Luci",
    "Technical Time Traveler - Luci",
    "Bug Tester - Luci",
    "Chief Interdimensional Transporter - Luci",
    "Time-Based Coffee Expert - Luci",
    "Universe Organizer - Luci",
    "Chronological Secretary - Luci",
    "TARDIS Plumber - Luci",
    "Hologram Engineer - Luci",
    "Emergency Time Disruptor - Luci",
    "Space-Time Decorator - Luci",
    "Anti-Matter Manager - Luci",
    "Crisis Management Specialist - Luci",
    "Temporal Rewind Expert - Luci",
    "Galactic Researcher - Luci",
    "Time Lord Liaison - Luci",
    "Dimensional Security Officer - Luci",
    "Time Paradox Solver - Luci",
    "Time Traveler’s Wardrobe Consultant - Luci",
    "Space-Time Contortionist - Luci",
    "TARDIS Key Holder - Luci",
    "Official Sonic Screwdriver Tester - Luci",
    "Alien Language Interpreter - Luci",
    "The Master’s Personal Assistant - Luci",
    "Temporal Rescue Team - Luci",
    "Rogue Time Traveler - Luci",
    "Universe Curator - Luci",
    "Time-Lord Code Monkey - Luci",
    "Big Bang Rewind Specialist - Luci",
    "Time Traveling Intern - Luci",
    "Doctor’s Ghostwriter - Luci",
    "Time Paradox Prevention Officer - Luci",
    "Office Plant Waterer - Luci",
    "Planetary Systems Engineer - Luci",
    "Dimensional Sandwich Artist - Luci",
    "TARDIS Stealth Tech - Luci",
    "Space-Time Event Planner - Luci",
    "Cosmic Courier - Luci",
    "Gallifreyan Diplomat - Luci",
    "Gallifreyan Carpet Designer - Luci",
    "Multi-verse Logistics Manager - Luci",
    "Official TARDIS Sweeper - Luci",
    "TARDIS Flight Attendant - Luci",
    "Space-Time Lawyer - Luci",
    "Alien Relations Specialist - Luci",
    "Time-Lord Fashion Consultant - Luci",
    "Time Rip Specialist - Luci",
    "Chrono-Ethics Advisor - Luci",
    "Interdimensional Sound Technician - Luci",
    "Official Companion - Luci",
    "Vortex Explorer - Luci",
    "The Zygon Wrangler - Luci",
    "Time Warp Intern - Luci",
    "Temporal Scribe - Luci",
    "Doctor’s Sidekick - Luci",
    "Timeline Repair Specialist - Luci",
    "Vortex Surveillance Officer - Luci",
    "Temporal Culinary Expert - Luci",
    "Dimensional Data Analyst - Luci",
    "The Time Lord’s Personal Barber - Luci",
    "Gallifreyan Concierge - Luci",
    "Chrono-Painter - Luci",
    "Interdimensional Photo Editor - Luci",
    "Temporal Feedback Coordinator - Luci",
    "All-Seeing Office Supervisor - Luci",
    "TARDIS Maintenance Crew - Luci",
    "The Ever-Expanding Time Creator - Luci",
    "Architect of the Infinite Vortex - Luci",
    "Guardian of Timelines - Luci",
    "Master of Time Itself - Luci",
    "Time Lord of Code and Chaos - Luci",
    "Keeper of Cosmic Order - Luci",
    "The Architect of the Time Vortex - Luci", "", "", "", "", "", "", "", "", "", "Master Baiter - Jamie"
};
    public float spacing = 50f; // Spacing between credit lines
    public float endMoveSpeed = 50f; // Speed at which "The End" moves up
    public float creditsScrollSpeed = 30f; // Speed of credits scrolling
    public float endPauseDuration = 2f; // How long "The End" stays in place
    public float logoMoveSpeed = 30f; // Speed for the game title rising

    public bool creditsStarted = false;
    public GameObject[] spawnedCredits; // To store instantiated credits

    void Start()
    {
        GameTitle.gameObject.SetActive(false); // Hide the game title at start
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