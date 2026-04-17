using UnityEngine;

[CreateAssetMenu(fileName = "NewCollectible", menuName = "Systems/Collectible Data")]
public class CollectItem : ScriptableObject
{
    [Header("Collectible Settings")]
    public string CollectibleCode;
    public string ItemName;
    public enum CollectibleType { QuestItem, Note, AudioLog, OST , Art }
    public CollectibleType Type;

    [Header("Type 1: Quest Item")] // collected in stages, used in stages
    public bool IsQuestItem; 

    [Header("Type 2: Note")] // collected in stages
    [TextArea(1, 25)] 
    public string NoteText;

    [Header("Type 3: Audio Log")] // collected in stages
    public AudioClip AudioLogClip;

    [Header("Type 4: Sound Track")]
    public AudioClip OSTClip;

    [Header("Type 5: Art")] // collected after stages and during filler stages
    public Sprite GalleryImage;
}
