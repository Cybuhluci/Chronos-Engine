using UnityEngine;

[CreateAssetMenu(fileName = "LootSO", menuName = "Scriptable Objects/LootSO")]
public class LootSO : ScriptableObject
{
    public string lootName;
    public GameObject lootBagPrefab;
    public int lootCost;
    public float lootEXP;
}
