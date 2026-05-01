using UnityEngine;

[System.Serializable]
public class FactionReputation
{
    public string factionID;
    public int positiveKarma;
    public int negativeKarma;

    public FactionReputation(string id, int posKarma = 0, int negKarma = 0)
    {
        factionID = id;
        positiveKarma = posKarma;
        negativeKarma = negKarma;
    }

    public void AddPositiveKarma(int amount)
    {
        positiveKarma += amount;
    }

    public void AddNegativeKarma(int amount)
    {
        negativeKarma += amount;
    }

    public int GetNetKarma()
    {
        return positiveKarma - negativeKarma;
    }
}
