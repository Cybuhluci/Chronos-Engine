using System.Collections;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    [SerializeField] private int bagsCollected;
    [SerializeField] private int minimumBagsToLeave = 5;
    int funnyExitHeistDelay = 5; // a delay before ending that allows the player to run off and do funny shit before the heist actually ends
    int heistdelaycounter;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    public void TryExitHeist()
    {
        if (IsHeistLeavable())
        {
            StartCoroutine(ExitHeistAfterDelay());
            StageManager.Instance.LoadMiscScene("mainmenu");
        }
    }

    public void AddBags(int amount, GameObject bag)
    {
        bagsCollected += amount;
        Destroy(bag);
    }

    public bool IsHeistLeavable()
    {
        return bagsCollected >= minimumBagsToLeave;
    }

    private IEnumerator ExitHeistAfterDelay()
    {
        yield return new WaitForSecondsRealtime(funnyExitHeistDelay);
    }
}