using UnityEngine;

public class PDAWeapons : MonoBehaviour
{
    public GunMainScript gunMainScript;

    public void EquipWeaponFromSO(MainGunDataSO mainGunDataSO)
    {
        gunMainScript.EquipWeaponNow(mainGunDataSO, null);
    }
}
