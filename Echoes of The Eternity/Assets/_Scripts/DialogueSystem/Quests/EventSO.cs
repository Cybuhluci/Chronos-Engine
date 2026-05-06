using UnityEngine;

[CreateAssetMenu(fileName = "EventSO", menuName = "Dialogue/Quests/EventSO")]
public class EventSO : ScriptableObject
{
    public string eventID;
    public string eventName;
    public int eventValue; // 0 or 1 (being done or not, or you can use it for other purposes as needed)
}
