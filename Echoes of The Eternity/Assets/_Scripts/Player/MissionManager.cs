using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    public enum HeistStage
    {
        Stealth, // no spawns
        Control, // low spawns
        Anticipation, // increasing spawns
        Assault, // high spawns
        Fade // decreasing spawns
    }
    public HeistStage currentHeistStage = HeistStage.Stealth;

    public enum PlayerState
    {
        Casing, // the player is casing the joint, no spawns, no combat music, etc
        Masked, // the player is actively doing the heist, spawns and music are active
    }
    public PlayerState currentPlayerState = PlayerState.Casing;

    public enum PlayerLocation
    {
        Public, // the player is in a public area, they are in a public area.
        Private, // the player is in a private area, they are somewhere they shouldn't really be - not a big deal, but they should be careful.
        Secure // the player is in a secure area, they should not be here at all.
    }
    public PlayerLocation currentPlayerLocation = PlayerLocation.Public;

    public enum Difficulty
    {
        Easy, 
        Normal, 
        Hard, 
        VeryHard, 
        Overkill,
        Mayhem,
    }
    public Difficulty currentDifficulty = Difficulty.Normal;

    [SerializeField] private int bagsCollected;
    [SerializeField] private int minimumBagsToLeave = 5;
    int funnyExitHeistDelay = 5; // a delay before ending that allows the player to run off and do funny shit before the heist actually ends
    int heistdelaycounter;

    [SerializeField] private TMP_Text bagcounter, assaulPhaseText;

    [SerializeField] private PlayerInput _PlayerInput;
    [SerializeField] private GunMainScript _GunMainScript;
    bool maskedUp;

    [SerializeField] private TMP_Text sustext;

    [SerializeField] private ResultsScreen _resultsScreen;

    private void Update()
    {
        bagcounter.text = $"Bags Secured: {bagsCollected}";
        assaulPhaseText.text = $"{currentHeistStage}";

        ManagePhases();

        // spherecast to check for the nearest enemy and check the susmeter to put onto the sustext tmptext.
        UpdateNearestEnemySuspicion();
    }

    public float timer;
    public float controlPhaseDuration = 60f; // 60
    public float anticipationPhaseDuration = 20f; // 20
    public float assaultPhaseDuration = 180f; // 180
    public float fadePhaseDuration = 30f; // 30
    private void ManagePhases()
    {
        // this function will change phases based on timers and conditions.
        // For example, if the player is in control phase, after 1 minute it will switch to anticipation phase.
        // If the player is in anticipation phase for 20 seconds, it will switch to assault phase,
        // and assault -> fade in 3 minutes, then it repeats from control until the heist ends.

        if (currentHeistStage == HeistStage.Stealth) return; // don't start the phase timer until the player fucks up stealth
        timer += Time.deltaTime;
        if (currentHeistStage == HeistStage.Control && timer >= controlPhaseDuration)
        {
            ChangeHeistState(HeistStage.Anticipation);
            timer = 0f;
        }
        else if (currentHeistStage == HeistStage.Anticipation && timer >= anticipationPhaseDuration)
        {
            ChangeHeistState(HeistStage.Assault);
            timer = 0f;
        }
        else if (currentHeistStage == HeistStage.Assault && timer >= assaultPhaseDuration)
        {
            ChangeHeistState(HeistStage.Fade);
            timer = 0f;
        }
        else if (currentHeistStage == HeistStage.Fade && timer >= fadePhaseDuration)
        {
            ChangeHeistState(HeistStage.Control);
            timer = 0f;
        }
    }

    private void UpdateNearestEnemySuspicion()
    {
        // perform a small sphere overlap to find nearby enemies, then read their suspicion if they have an EnemyController
        float checkRadius = 20f;
        Collider[] hits = Physics.OverlapSphere(transform.position, checkRadius);
        float maxSus = 0f;
        float nearest = float.MaxValue;
        foreach (var c in hits)
        {
            var ec = c.GetComponentInParent<EnemyController>();
            if (ec == null) continue;
            float dist = Vector3.Distance(transform.position, ec.transform.position);
            if (dist < nearest)
            {
                nearest = dist;
                maxSus = ec.GetSuspicion();
            }
        }

        if (sustext != null)
        {
            if (nearest == float.MaxValue)
                sustext.text = "No nearby enemies";
            else
                sustext.text = $"Highest enemy suspicion: {maxSus:0.0} (dist {nearest:0.0}m)";
        }
    }

    public void ChangeHeistState(HeistStage state)
    {
        currentHeistStage = state;

        switch (state)
        {
            case HeistStage.Stealth:
                break;
            case HeistStage.Control:
                break;
            case HeistStage.Anticipation:
                break;
            case HeistStage.Assault:
                break;
            case HeistStage.Fade:
                break;
            default:
                break;
        }
    }

    public HeistStage GetHeistStage()
    {
        return currentHeistStage;
    }

    public PlayerState GetPlayerState()
    {
        return currentPlayerState;
    }

    public void PullAlarm()
    {
        ChangeHeistState(HeistStage.Control);
    }

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
        _resultsScreen.StartResults();
    }
}