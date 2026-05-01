using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Dialogue/CharacterData")]
public class CharacterData : ScriptableObject
{
    public string characterName;

    public DialogueSO dialogue;

    public InventoryItemSO[] inventoryItems;
 
    public InventoryItemSO[] GetInventoryItems()
    {
        return inventoryItems;
    }
}
