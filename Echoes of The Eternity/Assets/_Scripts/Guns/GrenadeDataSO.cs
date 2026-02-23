using UnityEngine;

[CreateAssetMenu(fileName = "GrenadeDataSO", menuName = "Scriptable Objects/GrenadeDataSO")]
public class GrenadeDataSO : ScriptableObject
{
    public string weaponName;
    public GameObject model;

    public GrenadeType grenadeType;
    public int damage;
    public float blastRadius;
    public float fuseTime; // Time before the grenade explodes after being thrown
    // force of throws will be calculated in a controller script so it is consistent across all grenades.

    public enum GrenadeType
    {
        Frag,
        Incendiary,
        Smoke,
        Flash,
        Stun,
    }
}
