using UnityEngine;

[CreateAssetMenu(fileName = "AmmoTypesSO", menuName = "Scriptable Objects/AmmoTypesSO")]
public class AmmoTypesSO : ScriptableObject
{
    public float damageMult; // mult for damage. (e.g. 1.5 means 150% damage)
    public float conditionMult; // mult for condition. (e.g. 1.2 means 120% condition)
    public float _DTDam; // damage taken by armour. (e.g. -15 means 15 damage reduction)
    public float _DRMult; // mult for damage resistance. (e.g. 3 means 300% damage increase against unarmoured or light armour enemies)
}
