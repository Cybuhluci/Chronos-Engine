using System.Collections.Generic;
using UnityEngine;

public static class EventDatabase
{
    private static Dictionary<string, EventSO> events;
    public static bool IsInitialized { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        events = new Dictionary<string, EventSO>();
        
        // Load all EventSOs from any Resources folder
        var allEvents = Resources.LoadAll<EventSO>("");
        
        foreach (var ev in allEvents)
        {
            if (string.IsNullOrEmpty(ev.eventID))
            {
                Debug.LogWarning($"EventSO '{ev.name}' is missing an eventID!");
                continue;
            }

            if (!events.ContainsKey(ev.eventID))
            {
                events.Add(ev.eventID, ev);
            }
            else
            {
                Debug.LogWarning($"Duplicate event ID found: {ev.eventID} on '{ev.name}'");
            }
        }
        
        IsInitialized = true;
        Debug.Log($"Event Database Initialized with {events.Count} events.");
    }

    public static bool GetEvent(string id, out EventSO ev)
    {
        ev = null;
        if (events == null || string.IsNullOrEmpty(id))
        {
            return false;
        }
        return events.TryGetValue(id, out ev);
    }
}
