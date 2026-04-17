using UnityEngine;

[CreateAssetMenu(fileName = "AmmoTypesSO", menuName = "Scriptable Objects/AmmoTypesSO")]
public class AmmoTypesSO : ScriptableObject
{
    public string ammoName = "Ammo Name";
    public int damage;
    public float damageMult = 1; // mult for damage. (e.g. 1.5 means 150% damage)
    public float conditionMult = 1; // mult for condition. (e.g. 1.2 means 120% condition)
    public int _DTDam = 0; // damage taken by armour. (e.g. -15 means 15 damage reduction)
    public float _DRMult = 1; // mult for damage resistance. (e.g. 3 means 300% damage increase against unarmoured or light armour enemies)
    public int bulletsPerShot = 1; // number of bullets fired per shot, allows shotguns and split cartridges. (e.g. 2 means the ammo is a "two-shot")
}