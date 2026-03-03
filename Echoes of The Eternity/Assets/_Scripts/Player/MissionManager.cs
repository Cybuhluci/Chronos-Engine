using System.Collections;
using TMPro;
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

    [SerializeField] private int bagsCollected;
    [SerializeField] private int minimumBagsToLeave = 5;
    int funnyExitHeistDelay = 5; // a delay before ending that allows the player to run off and do funny shit before the heist actually ends
    int heistdelaycounter;

    [SerializeField] private TMP_Text bagcounter;

    [SerializeField] private PlayerInput _PlayerInput;
    [SerializeField] private GunMainScript _GunMainScript;
    bool maskedUp;

    [SerializeField] private TMP_Text sustext;

    private void Update()
    {
        bagcounter.text = $"Bags Secured: {bagsCollected}";
        if (!maskedUp)
        {
            if (_PlayerInput.actions["Quicknade"].WasPressedThisFrame())
            {
                maskedUp = true;
                _GunMainScript.MaskUp();
                currentPlayerState = PlayerState.Masked;
            }
        }

        // spherecast to check for the nearest enemy and check the susmeter to put onto the sustext tmptext.
        UpdateNearestEnemySuspicion();
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
                sustext.text = $"Nearest enemy suspicion: {maxSus:0.0} (dist {nearest:0.0}m)";
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