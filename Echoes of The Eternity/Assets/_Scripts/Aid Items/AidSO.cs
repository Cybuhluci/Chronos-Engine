using UnityEngine;

[CreateAssetMenu(fileName = "AidSO", menuName = "Luci/Item/Aid/AidSO")]
public class AidSO : ScriptableObject
{
    public string aidName;
    public int value;
    public float weight;
    public AidEffectSO effect;

    public enum AidType
    {
        Food,
        Drink,
        Medicine,
        Drug,
    }
    public AidType aidType;
}
