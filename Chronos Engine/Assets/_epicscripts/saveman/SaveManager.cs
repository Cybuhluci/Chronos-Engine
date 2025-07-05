using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private static string savePath => Application.persistentDataPath + "/Save/SaveFile.json";
    public static SaveData CurrentSave = new SaveData();

    private CollectItem Item;

    public static void SaveGame()
    {
        string json = JsonUtility.ToJson(CurrentSave, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Game Saved");
    }

    public static void LoadGame()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            CurrentSave = JsonUtility.FromJson<SaveData>(json);
            Debug.Log($"Game Loaded - {CurrentSave.collectedItems.Count} items");
        }
        else
        {
            Debug.Log("No save file found. Starting fresh.");
            CurrentSave = new SaveData();
        }
    }

    public static void ResetGame()
    {
        CurrentSave = new SaveData();
        SaveGame();
        Debug.Log("Game Reset");
    }

    public static bool HasCollected(string code)
    {
        return CurrentSave.collectedItems.Exists(item => item.CollectibleCode == code);
    }


    public static void Collect(CollectItem item)
    {
        if (!HasCollected(item.CollectibleCode))
        {
            var collectedData = new CollectedItemData
            {
                CollectibleCode = item.CollectibleCode,
                ItemName = item.ItemName,
                Type = item.Type.ToString(),

                NoteText = item.NoteText,
                AudioLogClipPath = item.AudioLogClip ? item.AudioLogClip.name : null,
                OSTClipPath = item.OSTClip ? item.OSTClip.name : null,
                GalleryImagePath = item.GalleryImage ? item.GalleryImage.name : null
            };

            CurrentSave.collectedItems.Add(collectedData);
            SaveGame();
        }
    }
}

[System.Serializable]
public class SaveData
{
    public List<CollectedItemData> collectedItems = new List<CollectedItemData>();
}

[System.Serializable]
public class CollectedItemData
{
    public string CollectibleCode;
    public string ItemName;
    public string Type;

    public string NoteText;
    public string AudioLogClipPath;
    public string OSTClipPath;
    public string GalleryImagePath;
}

