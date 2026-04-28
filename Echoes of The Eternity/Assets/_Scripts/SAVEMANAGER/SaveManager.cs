using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Luci.Saving
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private static string SkillsPath => Application.persistentDataPath + "/Save/SkillsFile.json";
        private static string PlayerPath => Application.persistentDataPath + "/Save/PlayerFile.json"; // PlayerAttributes.Instance
        public static List<SkillEntry> CurrentSkills = new();

        public void NewGame()
        {
            ResetSkills();
            Debug.Log("New Game Started");
        }

        public void ResetGame()
        {
            ResetSkills();
            Debug.Log("Game Reset");
        }

        public void LoadGame()
        {
            LoadSkills();
            Debug.Log("Game Loaded");
        }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
            LoadSkills();
            LoadPlayerStats();
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
}