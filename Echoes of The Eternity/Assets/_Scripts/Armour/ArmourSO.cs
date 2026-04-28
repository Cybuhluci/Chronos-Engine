using UnityEngine;

[CreateAssetMenu(fileName = "ArmourSO", menuName = "Luci/Item/Armour/ArmourSO")]
public class ArmourSO : ScriptableObject
{
    public string armourName;
    public int value;
    public float weight;
    public int _DT, _DR;
    public ArmourEffectSO effect;

    public enum ArmourClass
    {
        Helmet, 
        Light, 
        Medium, 
        Heavy, 
        SuperHeavy // this is power armour territory - and only power armour territory.
    }
    public ArmourClass armourClass;

    public enum ArmourType
    {
        Head, Torso
    }
    public ArmourType armourType;
}