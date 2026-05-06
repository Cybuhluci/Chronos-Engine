using UnityEngine;

[CreateAssetMenu(fileName = "QuestSO", menuName = "Dialogue/Quests/QuestSO")]
public class QuestSO : ScriptableObject
{
    public string questID;
    public string questName;
    public string questDescription;
    public int questValue;

    public EventSO[] associatedEvents;
}
