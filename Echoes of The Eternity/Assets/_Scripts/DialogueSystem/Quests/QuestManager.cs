using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    // Runtime dictionaries to track quest states
    private Dictionary<string, QuestStatus> questStates = new Dictionary<string, QuestStatus>();
    public enum QuestStatus { Inactive, Active, Completed, Failed }

    // Events for UI or other systems to listen to
    public event Action<QuestSO, QuestStatus> OnQuestStatusChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        // Subscribe to the EventManager's event
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnEventUpdated += HandleEventUpdated;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnEventUpdated -= HandleEventUpdated;
        }
    }

    public void StartQuest(string questID)
    {
        if (questStates.ContainsKey(questID) && questStates[questID] != QuestStatus.Inactive)
        {
            Debug.LogWarning($"Quest '{questID}' is already started or completed.");
            return;
        }

        var quest = GetQuestByID(questID);
        if (quest != null)
        {
            SetQuestStatus(questID, QuestStatus.Active);
        }
    }

    private void HandleEventUpdated(string eventID)
    {
        // Check all active quests to see if this event update affects them
        foreach (var questEntry in questStates)
        {
            if (questEntry.Value == QuestStatus.Active)
            {
                var quest = GetQuestByID(questEntry.Key);
                if (quest != null && quest.associatedEvents.Any(e => e.eventID == eventID))
                {
                    CheckQuestCompletion(quest);
                }
            }
        }
    }

    private void CheckQuestCompletion(QuestSO quest)
    {
        if (quest == null || quest.associatedEvents == null) return;

        bool allEventsCompleted = true;
        foreach (var reqEvent in quest.associatedEvents)
        {
            int currentEventValue = EventManager.Instance.GetEventValue(reqEvent.eventID);
            if (currentEventValue < reqEvent.eventValue) // Assuming eventValue is the target value
            {
                allEventsCompleted = false;
                break;
            }
        }

        if (allEventsCompleted)
        {
            SetQuestStatus(quest.questID, QuestStatus.Completed);
            // You could also give rewards here
        }
    }

    public QuestStatus GetQuestStatus(string questID)
    {
        if (questStates.TryGetValue(questID, out QuestStatus status))
        {
            return status;
        }
        return QuestStatus.Inactive;
    }

    private void SetQuestStatus(string questID, QuestStatus newStatus)
    {
        var quest = GetQuestByID(questID);
        if (quest == null) return;

        questStates[questID] = newStatus;
        Debug.Log($"Quest '{quest.questName}' status changed to: {newStatus}");
        OnQuestStatusChanged?.Invoke(quest, newStatus);
    }

    private QuestSO GetQuestByID(string questID)
    {
        if (QuestDatabase.GetQuest(questID, out QuestSO quest))
        {
            return quest;
        }
        Debug.LogWarning($"QuestManager: Quest with ID {questID} not found in QuestDatabase.");
        return null;
    }

    // --- Save/Load ---
    public Dictionary<string, QuestStatus> GetQuestStatesForSave()
    {
        return questStates;
    }

    public void LoadQuestStates(Dictionary<string, QuestStatus> loadedStates)
    {
        questStates = loadedStates ?? new Dictionary<string, QuestStatus>();
    }
}

