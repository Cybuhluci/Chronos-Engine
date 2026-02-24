using UnityEngine;

[CreateAssetMenu(fileName = "SignatureDeployableSO", menuName = "Scriptable Objects/SignatureDeployableSO")]
public class SignatureDeployableSO : ScriptableObject
{
    public string weaponName;
    public GameObject model;

    public int Ammo;
    public int cooldownTime; // Time in seconds before the deployable can be used again after being deployed.
}
