using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLevelSystem : MonoBehaviour
{
    public static PlayerLevelSystem Instance { get; private set; }
    public int currentLevel = 1;
    public float currentXP = 0;
    public float xpToNextLevel => (25 * (3 * currentLevel+ 2 ) * (currentLevel - 1)); 
    public float tmpxptonextlvl;

    private PlayerInput playerInput;

    private void Awake()
    {
        Instance = this;
        playerInput = FindAnyObjectByType<PlayerInput>();
    }

    private void Update()
    {
        // for testing purposes, press L to gain 50 XP.
        if (playerInput.actions["Quicknade"].WasPerformedThisFrame())
        {
            GainXP(50);
        }

        tmpxptonextlvl = xpToNextLevel;
    }

    public void GainXP(float amount)
    {
        currentXP += amount;
        CheckLevelUp();
    }

    public void SetLevel(int level)
    {
        currentLevel = Mathf.Max(1, currentLevel); // Ensure level is at least 1
        currentXP = 0; // Reset XP when setting a new level
    }

    private void CheckLevelUp()
    {
        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            currentLevel++;
        }
    }
}