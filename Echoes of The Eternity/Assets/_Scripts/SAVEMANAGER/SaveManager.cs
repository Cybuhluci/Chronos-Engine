using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Luci.Saving
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private static string SkillsPath => Application.persistentDataPath + "/Save/SkillsFile.json";
        private static string PlayerPath => Application.persistentDataPath + "/Save/PlayerFile.json";
        private static string InventoryPath => Application.persistentDataPath + "/Save/InventoryFile.json";
        private static string FactionReputationsPath => Application.persistentDataPath + "/Save/Factions.json";
        public static List<SkillEntry> CurrentSkills = new();

        public void NewGame()
        {
            ResetSkills();
            // Note: You might want to reset player stats and factions here too
            Debug.Log("New Game Started");
        }

        public void ResetGame()
        {
            ResetSkills();
            // Note: You might want to reset player stats and factions here too
            Debug.Log("Game Reset");
        }

        public void LoadGame()
        {
            LoadSkills();
            LoadPlayerStats();
            LoadInventory();

            if (global::FactionManager.Instance != null)
            {
                global::FactionManager.Instance.ReloadReputations();
            }

            Debug.Log("Game Loaded");
        }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
            LoadGame();
        }

        public void SaveFactionReputations(Dictionary<string, FactionReputation> reputations)
        {
            CreateSaveDirectoryIfNeeded();
            var saveData = new FactionReputationSaveData();
            saveData.reputations = new List<FactionReputation>(reputations.Values);

            // Sort the list by factionID numerically/alphabetically before saving
            saveData.reputations.Sort((a, b) => string.Compare(a.factionID, b.factionID));

            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(FactionReputationsPath, json);
            Debug.Log("Faction reputations saved.");
        }

        public List<FactionReputation> LoadFactionReputations()
        {
            if (!File.Exists(FactionReputationsPath))
            {
                Debug.Log("No faction reputation save file found.");
                return null;
            }

            string json = File.ReadAllText(FactionReputationsPath);
            FactionReputationSaveData saveData = JsonUtility.FromJson<FactionReputationSaveData>(json);
            Debug.Log("Faction reputations loaded.");
            return saveData.reputations;
        }

        public void SaveSTRIVEStats()
        {
            if (PlayerAttributes.Instance == null)
            {
                Debug.LogWarning("PlayerAttributes instance not found. Cannot save stats.");
                return;
            }

            CreateSaveDirectoryIfNeeded();

            var statsData = new PlayerStatsData
            {
                sway = PlayerAttributes.Instance.sway,
                tenacity = PlayerAttributes.Instance.tenacity,
                rapidity = PlayerAttributes.Instance.rapidity,
                intellect = PlayerAttributes.Instance.intellect,
                vitality = PlayerAttributes.Instance.vitality,
                eye = PlayerAttributes.Instance.eye
            };

            string json = JsonUtility.ToJson(statsData, true);
            File.WriteAllText(PlayerPath, json);
            Debug.Log("Player STRIVE stats saved.");
        }

        public void LoadPlayerStats()
        {
            if (!File.Exists(PlayerPath))
            {
                Debug.Log("No player stats save file found.");
                return;
            }

            string json = File.ReadAllText(PlayerPath);
            PlayerStatsData statsData = JsonUtility.FromJson<PlayerStatsData>(json);

            if (PlayerAttributes.Instance != null)
            {
                PlayerAttributes.Instance.SetStat("sway", statsData.sway);
                PlayerAttributes.Instance.SetStat("tenacity", statsData.tenacity);
                PlayerAttributes.Instance.SetStat("rapidity", statsData.rapidity);
                PlayerAttributes.Instance.SetStat("intellect", statsData.intellect);
                PlayerAttributes.Instance.SetStat("vitality", statsData.vitality);
                PlayerAttributes.Instance.SetStat("eye", statsData.eye);
                Debug.Log("Player STRIVE stats loaded.");
            }
            else
            {
                Debug.LogWarning("PlayerAttributes instance not found. Cannot load stats.");
            }
        }

        public void SaveInventory(List<InventoryItemSO> inventoryItems)
        {
            CreateSaveDirectoryIfNeeded();
            var saveData = new InventorySaveData();
            var itemCounts = new Dictionary<string, int>();

            foreach (var item in inventoryItems)
            {
                if (itemCounts.ContainsKey(item.id))
                {
                    itemCounts[item.id]++;
                }
                else
                {
                    itemCounts[item.id] = 1;
                }
            }

            foreach (var pair in itemCounts)
            {
                saveData.items.Add(new InventoryItemEntry { itemID = pair.Key, quantity = pair.Value });
            }

            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(InventoryPath, json);
            Debug.Log("Inventory saved.");
        }

        public void LoadInventory()
        {
            if (!File.Exists(InventoryPath))
            {
                Debug.Log("No inventory save file found.");
                return;
            }

            string json = File.ReadAllText(InventoryPath);
            InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);

            if (InventoryManager.Instance != null && ItemDatabase.IsInitialized)
            {
                InventoryManager.Instance.ClearInventory();
                foreach (var itemEntry in saveData.items)
                {
                    if (ItemDatabase.GetItem(itemEntry.itemID, out InventoryItemSO itemSO))
                    {
                        for (int i = 0; i < itemEntry.quantity; i++)
                        {
                            InventoryManager.Instance.AddItem(itemSO);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Could not find item with ID '{itemEntry.itemID}' in the database.");
                    }
                }
                Debug.Log("Inventory loaded.");
            }
            else
            {
                Debug.LogWarning("InventoryManager or ItemDatabase not ready. Cannot load inventory.");
            }
        }

        // Save skills (individual skills' unlock states)
        public static void SaveSkills()
        {
            CreateSaveDirectoryIfNeeded();

            var data = new SkillSaveData { unlockedSkills = CurrentSkills };
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SkillsPath, json);
            Debug.Log("Skills Saved");
        }

        // Load skills (individual skills' unlock states)
        public static void LoadSkills()
        {
            CreateSaveDirectoryIfNeeded();

            if (!File.Exists(SkillsPath))
            {
                Debug.Log("No skill save found. Starting fresh.");
                CurrentSkills = new List<SkillEntry>();
                return;
            }

            string json = File.ReadAllText(SkillsPath);
            SkillSaveData data = JsonUtility.FromJson<SkillSaveData>(json);
            CurrentSkills = data.unlockedSkills ?? new List<SkillEntry>();
            Debug.Log($"Skills Loaded: {CurrentSkills.Count} entries");
        }

        // Reset skills (clear saved data)
        public static void ResetSkills()
        {
            CurrentSkills.Clear();
            SaveSkills();
            Debug.Log("Skills Reset");
        }

        private static void CreateSaveDirectoryIfNeeded()
        {
            string dir = Application.persistentDataPath + "/Save";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                Debug.Log("Save directory created");
            }
        }
    }

    [System.Serializable]
    public class InventorySaveData
    {
        public List<InventoryItemEntry> items = new List<InventoryItemEntry>();
    }

    [System.Serializable]
    public class InventoryItemEntry
    {
        public string itemID;
        public int quantity;
    }

    [System.Serializable]
    public class PlayerStatsData
    {
        public int sway;
        public int tenacity;
        public int rapidity;
        public int intellect;
        public int vitality;
        public int eye;
    }

    [System.Serializable]
    public class SkillEntry
    {
        public string skillID;  // The ID of the skill (from the Skill class)
        public bool isUnlocked; // Whether the skill is unlocked or not
    }

    [System.Serializable]
    public class SkillSaveData
    {
        public List<SkillEntry> unlockedSkills = new();
    }

    [System.Serializable]
    public class FactionReputationSaveData
    {
        public List<global::FactionReputation> reputations = new();
    }
}