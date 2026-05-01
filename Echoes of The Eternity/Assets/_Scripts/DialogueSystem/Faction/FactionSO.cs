using UnityEngine;

[CreateAssetMenu(fileName = "FactionSO", menuName = "Luci/Faction/FactionSO")]
public class FactionSO : ScriptableObject
{
    public string factionID; // unique identifier for the faction, e.g. 0001, 0002 0101,etc. 

    public enum FactionType
    {
        Neutral, // won't attack player unless provoked
        Hostile // will attack player on sight
    }
    public FactionType factionType;

    public enum Faction
    {
        // base game - tutorial + mission 1
        Equilibrium, // 0000
        Eternity, // 0001
        Caelus4c, // 0002
        Carthage4d, // 0003
        Daleks, // 0004
        // dlc 1
        TheUniversity, // 0100
        NightVale, // 0101
        Librarians, // 0102
        Baristas, // 0103
        // mission 2
        TheGifted, // 0200
        TheUtops, // 0201
        TheArchs, // 0202
    }
    public Faction faction;

    public Faction[] willAttack;
    public Faction[] willRunFrom;

    public int positiveKarma;
    public int negativeKarma;
    
    public void AddPositiveKarma(int amount)
    {
        positiveKarma += amount;
    }
    
    public void AddNegativeKarma(int amount)
    {
        negativeKarma += amount;
    }
}
