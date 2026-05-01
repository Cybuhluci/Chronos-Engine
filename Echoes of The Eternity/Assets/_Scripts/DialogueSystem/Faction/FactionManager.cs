using Luci.Saving;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FactionManager : MonoBehaviour
{
    public static FactionManager Instance { get; private set; }

    private Dictionary<string, FactionSO> factionDatabase = new Dictionary<string, FactionSO>();
    private Dictionary<string, FactionReputation> playerReputations = new Dictionary<string, FactionReputation>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAllFactionSOs();
    }

    void Start()
    {
        InitializeReputations();
    }

    private void LoadAllFactionSOs()
    {
        factionDatabase.Clear();
        var allFactions = Resources.LoadAll<FactionSO>("Factions");
        foreach (var faction in allFactions)
        {
            if (!factionDatabase.ContainsKey(faction.factionID))
            {
                factionDatabase.Add(faction.factionID, faction);
            }
            else
            {
                Debug.LogWarning($"Duplicate FactionID found: {faction.factionID} on {faction.name}");
            }
        }
        Debug.Log($"Loaded {factionDatabase.Count} factions into the database.");
    }

    private void InitializeReputations()
    {
        playerReputations.Clear();
        
        if (Luci.Saving.SaveManager.Instance == null)
        {
            Debug.LogWarning("SaveManager Instance is null! Cannot load factions.");
            return;
        }

        var loadedReps = Luci.Saving.SaveManager.Instance.LoadFactionReputations();

        if (loadedReps != null && loadedReps.Count > 0)
        {
            // Load from save file
            foreach (var rep in loadedReps)
            {
                playerReputations[rep.factionID] = rep;
            }
            Debug.Log("Player reputations loaded from save.");
        }
        else
        {
            // No save file, create from defaults
            foreach (var pair in factionDatabase)
            {
                playerReputations.Add(pair.Key, new FactionReputation(pair.Key, pair.Value.positiveKarma, pair.Value.negativeKarma));
            }
            Debug.Log("No save file found. Initialized player reputations from defaults.");
            
            // Immediately save the defaults so the file isn't empty
            Luci.Saving.SaveManager.Instance.SaveFactionReputations(playerReputations);
        }
    }

    public void ReloadReputations()
    {
        InitializeReputations();
    }

    public void AddKarma(string factionID, int positiveAmount, int negativeAmount)
    {
        if (playerReputations.TryGetValue(factionID, out FactionReputation reputation))
        {
            reputation.AddPositiveKarma(positiveAmount);
            reputation.AddNegativeKarma(negativeAmount);
            SaveManager.Instance.SaveFactionReputations(playerReputations);
        }
        else
        {
            Debug.LogWarning($"Attempted to add karma to a non-existent faction: {factionID}");
        }
    }

    public FactionReputation GetReputation(string factionID)
    {
        if (playerReputations.TryGetValue(factionID, out FactionReputation reputation))
        {
            return reputation;
        }
        return null;
    }

    public Dictionary<string, FactionReputation> GetAllReputations()
    {
        return playerReputations;
    }

    public int GetNetKarma(string factionID)
    {
        var reputation = GetReputation(factionID);
        return reputation != null ? reputation.GetNetKarma() : 0;
    }

    public FactionSO.FactionType GetFactionAttitude(string factionID)
    {
        if (factionDatabase.TryGetValue(factionID, out FactionSO faction))
        {
            // Future logic can go here to modify attitude based on karma
            return faction.factionType;
        }
        return FactionSO.FactionType.Neutral;
    }

    public bool DoesFactionAttack(FactionSO.Faction attacker, FactionSO.Faction target)
    {
        var attackerSO = factionDatabase.Values.FirstOrDefault(f => f.faction == attacker);
        if (attackerSO != null)
        {
            return attackerSO.willAttack.Contains(target);
        }
        return false;
    }
}