using UnityEngine;

[CreateAssetMenu(fileName = "GadgetDataSO", menuName = "Scriptable Objects/GadgetDataSO")]
public class GadgetDataSO : ScriptableObject
{
    public GadgetType gadgetType;
    public string weaponName;
    public GameObject model;

    public int damage;
    public float headshotMultiplier = 1.5f;
    public float fireRate; // Rounds per minute
    public FireMode fireMode;

    public int ammoStart;
    public int ammoMax;

    public float equipTime; // Time to equip the weapon
    public float dequipTime; // Time to unequip the weapon
    public float reloadTime; // Time to reload the weapon
    public float ADSTime; // Time to aim down sights
    public float recoil; // Recoil amount
    public float muzzleVelocity; // Muzzle velocity


    public enum GadgetType
    {
        Launcher,
        Deployable,
        HeavyWeapon,
        SpecialWeapon,
    }

    public enum FireMode
    {
        SemiAuto,
        FullAuto,
        Burst
    }
}
