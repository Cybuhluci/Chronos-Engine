using UnityEngine;

[CreateAssetMenu(fileName = "UniqueDeployableSO", menuName = "Scriptable Objects/UniqueDeployableSO")]
public class UniqueDeployableSO : ScriptableObject
{
    public string uniqueDeployableName;
    public GameObject uniqueDeployablePrefab;

    public int startingAmmo; // how maby charges or ammo the thing statrs with.
    public int Ammo; // either how much health or battery the deployable has, or how long it takes to deplete (in the gas masks case)
    public int Chunks; // how many amounts of ammo the deployable has; only relevant for gas masks, which deplete in chunks
    public float equipTime = 1f; // time in seconds required to hold to equip/unequip

    public int maxChunks; // only relevant for gas masks, which have a max of 5 chunks

    public enum UniqueDeployableAmmoType
    {
        Charges, // gas masks - charges deplete in chunks; 5 chunks max; chunks can be replenished with support bags
        Health, // sunglasses - take damage when the player takes damage; 500 health max; can be replenished with support bags
        Battery // night vision - consumes battery power; 100 battery max; can be replenished with support bags
    }
}
