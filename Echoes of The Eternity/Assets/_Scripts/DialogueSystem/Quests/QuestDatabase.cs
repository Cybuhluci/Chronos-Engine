using System.Collections.Generic;
using UnityEngine;

public static class QuestDatabase
{
    private static Dictionary<string, QuestSO> quests;
    public static bool IsInitialized { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        quests = new Dictionary<string, QuestSO>();
        
        // Load all QuestSOs from any Resources folder
        var allQuests = Resources.LoadAll<QuestSO>("");
        
        foreach (var quest in allQuests)
        {
            if (string.IsNullOrEmpty(quest.questID))
            {
                Debug.LogWarning($"QuestSO '{quest.name}' is missing a questID!");
                continue;
            }

            if (!quests.ContainsKey(quest.questID))
            {
                quests.Add(quest.questID, quest);
            }
            else
            {
                Debug.LogWarning($"Duplicate quest ID found: {quest.questID} on '{quest.name}'");
            }
        }
        
        IsInitialized = true;
        Debug.Log($"Quest Database Initialized with {quests.Count} quests.");
    }

    public static bool GetQuest(string id, out QuestSO quest)
    {
        quest = null;
        if (quests == null || string.IsNullOrEmpty(id))
        {
            return false;
        }
        return quests.TryGetValue(id, out quest);
    }
}
