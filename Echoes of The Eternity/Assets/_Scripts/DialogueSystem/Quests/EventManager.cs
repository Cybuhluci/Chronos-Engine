using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    // Runtime dictionary to store the state of events.
    private Dictionary<string, int> eventStates = new Dictionary<string, int>();

    // Event that fires when an event's value changes. QuestManager will listen to this.
    public event Action<string> OnEventUpdated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Make it persist across scenes
        }
    }

    public void SetEvent(string eventID, int eventValue)
    {
        // First check if the event actually exists in the database
        if (!EventDatabase.GetEvent(eventID, out EventSO ev))
        {
            Debug.LogWarning($"EventManager: Attempted to set value for an unknown event ID: {eventID}");
            return;
        }

        eventStates[eventID] = eventValue;
        Debug.Log($"Event '{ev.eventName}' ({eventID}) state set to: {eventValue}");
        
        // Notify listeners that this event has been updated
        OnEventUpdated?.Invoke(eventID);
    }

    public int GetEventValue(string eventID)
    {
        if (eventStates.TryGetValue(eventID, out int value))
        {
            return value;
        }
        return 0; // Default to 0 if the event has no state yet
    }

    // Optional: Methods for saving/loading event states
    public Dictionary<string, int> GetEventStatesForSave()
    {
        return eventStates;
    }

    public void LoadEventStates(Dictionary<string, int> loadedStates)
    {
        eventStates = loadedStates ?? new Dictionary<string, int>();
    }
}


