using Luci.Interactions;
using System.Linq;
using TMPro;
using UnityEngine;

public class TimeLockDoor : MonoBehaviour
{
    private bool isCountdownActive = false; // Flag to indicate if the countdown is active
    public int requiredTime = 60; // The time required to unlock the door in seconds
    private float timer; // Timer to track the elapsed time
    [SerializeField] private TimeLockKCSlot[] keycardSlots; // Array of keycard slots that need to be filled

    private int insertedKeycards = 0; // Counter for the number of keycards inserted
    [SerializeField] private int keycardsRequired = 2; // Number of keycards required to unlock the door
    [SerializeField] private TMP_Text timelockcounter; // Reference to the TextMeshPro component to display the timer - countdown

    [SerializeField] private BasicKeyDoorScript doorScript; // Reference to the door script that will handle the unlocking logic

    private void Update()
    {
        if (isCountdownActive)
        {
            CountdownTimer();
        }
    }

    private void CountdownTimer()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime; // Decrease the timer by the time elapsed since the last frame
            timelockcounter.text = Mathf.Ceil(timer).ToString("F0"); // Update the displayed timer, rounding up to the nearest whole number
        }
        else
        {
            isCountdownActive = false; // Stop the countdown
            UnlockDoor(); // Unlock the door when the timer reaches zero
        }
    }

    public void InsertKeycard()
    {
        insertedKeycards++;
        if (insertedKeycards == keycardsRequired)
        {
            isCountdownActive = true; // Start the countdown
            timer = requiredTime; // Initialize the timer
        }
    }

    private void UnlockDoor()
    {
        doorScript.OpenDoor();
    }
}
