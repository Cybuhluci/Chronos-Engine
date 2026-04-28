using Luci.Saving;
using UnityEngine;

public class PlayerAttributes : MonoBehaviour
    // the most important script for the player.
    // it will hold all the attributes of the player, such as STRIVE stats, health, stamina, hunger, thirst, etc.
    // these are NOT the the variables used for currentplayer[blank], the health in this script is not current health, but instead the maxmimum health.
{
    public static PlayerAttributes Instance; // singleton instance of the PlayerAttributes script, so it can be accessed from other scripts.

    // STRIVE stats
    public int sway; // charisma
    public int tenacity; // strength
    public int rapidity; // agility
    public int intellect; // intelligence
    public int vitality; // endurance
    public int eye; // perception

    public int luck; // luck, affects all stats and random events.

    // other stats
    public int health; // health, if it reaches 0 the player is downed.
    public int stamina; // stamina, affects sprinting and melee attacks, if it reaches 0 the player is exhausted and cannot sprint or weapon bash until it regenerates.
    public int carryWeight; // carry weight, affects how much the player can carry before being encumbered, which affects movement speed and stamina regen.

    // statuses; if any reach 1000 the playuer dies, skipping downed state.
    public int thirst; // thirst, affects stamina regen and health regen.
    public int hunger; // hunger, affects stamina regen and health regen.
    public int sleep; // sleep, affects stamina regen and health regen.
    public int specialstatus; // special status; can be anything from Radiation to Mental State.

    private void Awake()
    {
        Instance = this; // set the singleton instance to this script, so it can be accessed from other scripts.
        // Initialize stats to default values foe debugging purposes. These can be changed later on as the player progresses.
        SaveManager.Instance.LoadPlayerStats(); // loads STRIVE stats from the save file, so they can be used in the game.
        luck = CalculateLuck();

        // things to update based on stats:
        // sway: nothing right now,
        // tenacity: carry weight and weapon handling,
        // rapidity: max SP and SP drain,
        // intellect: nothing right now,
        // vitality: max health, limb condition, and aid item hp regen,
        // eye: nothing right now,
        // luck: nothing right now.
    }

    private int CalculateLuck()
    {
        // Luck is calculated as the average of all STRIVE stats
        return (sway + tenacity + rapidity + intellect + vitality + eye) / 6;
    }

    private int CalculateCarryWeight()
    {
        // Carry weight is calculated based on tenacity, with a base carry weight of 50 and an additional 10 carry weight per point of tenacity.
        return 50 + (tenacity * 10);
    }

    private int CalculateMaxHealth()
    {
        // Max health is calculated based on vitality, with a base health of 100 and an additional 20 health per point of vitality.
        return 100 + (vitality * 20);
    }

    private int CalculateMaxStamina()
    {
        // Max stamina is calculated based on rapidity, with a base stamina of 65 and an additional 3 stamina per point of rapidity.
        return 65 + (rapidity * 3);
    }

    private void CalculateAllStats()
    {
        luck = CalculateLuck();
        carryWeight = CalculateCarryWeight();
        health = CalculateMaxHealth();
        stamina = CalculateMaxStamina();
    }

    public void SetStat(string name, int statamount)
    {
        switch (name.ToLower())
        {
            case "sway":
                sway = statamount;
                break;
            case "tenacity":
                tenacity = statamount;
                break;
            case "rapidity":
                rapidity = statamount;
                break;
            case "intellect":
                intellect = statamount;
                break;
            case "vitality":
                vitality = statamount;
                break;
            case "eye":
                eye = statamount;
                break;
            default:
                Debug.LogWarning("Invalid stat name: " + name);
                return;
        }
        CalculateAllStats(); // recalculate all stats that are affected by the changed stat, such as luck, carry weight, health, and stamina.
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveSTRIVEStats();
        }
    }

    public void PrintStats()
    {
        ConsoleManager.instance.AppendOutput($"Sway: {sway}\n" +
            $"Tenacity: {tenacity}\n" +
            $"Rapidity: {rapidity}\n" +
            $"Intellect: {intellect}\n" +
            $"Vitality: {vitality}\n" +
            $"Eye: {eye}\n" +
            $"Luck: {luck}");
    }
}
