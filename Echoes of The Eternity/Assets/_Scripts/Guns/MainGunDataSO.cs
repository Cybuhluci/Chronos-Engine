using UnityEngine;

[CreateAssetMenu(fileName = "MainGunDataSO", menuName = "Scriptable Objects/MainGunDataSO")]
public class MainGunDataSO : ScriptableObject
{
    public GunType gunType;
    public string weaponName;
    public GameObject model;

    public int damage;
    public float headshotMultiplier = 1.5f;
    public float fireRate; // Rounds per minute
    public FireMode fireMode;

    public int magazineSize;
    public int reserveAmmo;
    public int bulletsPerShot;

    public float equipTime; // Time to equip the weapon
    public float dequipTime; // Time to unequip the weapon
    public float reloadTime; // Time to reload the weapon
    public float ADSTime; // Time to aim down sights
    public float recoil; // Recoil amount
    public float spread; // Bullet spread
    public float muzzleVelocity; // Muzzle velocity

    public enum GunType
    {
        Pistol,
        Rifle,
        Shotgun,
        Sniper,
        SMG,
        LMG
    }

    public enum FireMode
    {
        SemiAuto,
        FullAuto,
        Burst
    }
}
