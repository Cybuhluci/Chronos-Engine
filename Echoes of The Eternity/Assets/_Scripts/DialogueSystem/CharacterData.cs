using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Dialogue/CharacterData")]
public class CharacterData : ScriptableObject
{
    public string characterName;

    public string temporaryDialogue;
    public int temporaryOptionsCount;
    public string[] temporaryOptions;

    public InventoryItemSO[] inventoryItems;
 
    public InventoryItemSO[] GetInventoryItems()
    {
        return inventoryItems;
    }
}
