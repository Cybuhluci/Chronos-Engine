using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using Luci;
using UnityEngine.UI;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;

// Player health and downed/self-revive behaviour
public class PlayerHealth : MonoBehaviour
{
    public bool _isInvulnerable = false;
    [Header("Health")]
    public float MaxHealth = 100f;
    public float CurrentHealth = 100f;
    public float MaxArmour = 100f;
    public float CurrentArmour = 100f;
    // Time of last received damage (Time.time). Used by healing logic to wait until player hasn't
    // taken damage for a short duration before starting passive heals.
    public float LastDamageTime { get; private set; } = -999f;

    // Armour is ablative: it absorbs damage until depleted, then health takes damage

    [Header("Downed / Self-Revive")]
    [Tooltip("Seconds player must hold Melee to self-revive")]
    public float selfReviveHoldTime = 5f;
    public Image selfReviveProgressUI; // optional UI element to show progress
    [Tooltip("If true player can self-revive once after being downed")]
    public bool allowSelfRevive = true;

    public bool IsAlive;

    [SerializeField] private bool _isDowned = false;
    [SerializeField] private bool _hasSelfRevived = false;
    [SerializeField] private float _reviveTimer = 0f;

    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private FirstPersonController _fpc;

    private void Awake()
    {
        CurrentHealth = MaxHealth;
    }

    private void Update()
    {
        if (!_isDowned) return;

        // Only allow self-revive if allowed and not yet used
        if (!allowSelfRevive || _hasSelfRevived) return;

        bool holding = false;
        if (_playerInput != null && _playerInput.actions["Melee"] != null)
        {
            holding = _playerInput.actions["Melee"].IsPressed();
        }
        else
        {
            holding = Keyboard.current != null && (Keyboard.current.rKey.isPressed || Keyboard.current.spaceKey.isPressed);
        }

        if (holding)
        {
            _reviveTimer += Time.deltaTime;
            if (_reviveTimer >= selfReviveHoldTime)
            {
                // complete self-revive
                _hasSelfRevived = true;
                ReviveSelf();
            }
        }
        else
        {
            // reset if released
            if (_reviveTimer > 0f) _reviveTimer = 0f;
        }

        selfReviveProgressUI.fillAmount = Mathf.Clamp01(_reviveTimer / selfReviveHoldTime);
    }

    public void TakeDamage(float amount)
    {
        if (_isInvulnerable) return;
        if (!_isDowned)
        {
            LastDamageTime = Time.time;
            if (CurrentArmour > 0f)
            {
                float armourDamage = Mathf.Min(CurrentArmour, amount);
                CurrentArmour -= armourDamage;
                amount -= armourDamage;
            }
            else if (CurrentArmour <= 0f)
            {
                CurrentHealth -= amount;
                CurrentArmour = 0f;
            }

            if (CurrentHealth <= 0f)
            {
                EnterDownedState();
            }
        }
    }

    public void Heal(float amount)
    {
        if (!_isDowned && IsAlive)
        {
            CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        }
    }

    public void HealOverTime()
    {
        StartCoroutine(HealOverTimeCoroutine());
    }

    IEnumerator HealOverTimeCoroutine()
    {
        new WaitForSeconds(1f);
        while (CurrentHealth <= MaxHealth)
        {
            CurrentHealth = Mathf.Min(CurrentHealth + 1 * (Time.deltaTime / 2), MaxHealth);
            yield return null;
        }
        yield break;
    }

    private void EnterDownedState()
    {
        IsAlive = false;
        _isDowned = true;
        _reviveTimer = 0f;

        // Disable player movement but allow camera so player can aim while downed
        if (_fpc != null)
        {
            _fpc.SetDownedState(true);
        }

        selfReviveProgressUI.gameObject.SetActive(true);
    }

    private void ReviveSelf()
    {
        // restore to half health on self-revive
        CurrentHealth = MaxHealth * 0.5f;
        IsAlive = true;
        _isDowned = false;
        _reviveTimer = 0f;

        if (_fpc != null)
        {
            _fpc.SetDownedState(false);
        }

        selfReviveProgressUI.gameObject.SetActive(false);
    }
}
