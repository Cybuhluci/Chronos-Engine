using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class DialogueChoice
{
    public string text;
    public string targetNodeId;
    public bool endsConversation;
}

public enum NodeType { Start, Dialogue, Choice, End, Action, Condition }
public enum EndNodeType { Normal, Attack, Trade }
public enum ActionType { Event, AddItem, RemoveItem, AddPositiveKarma, AddNegativeKarma }
public enum ConditionType { Event, FactionPositiveKarma, FactionNegativeKarma, PlayerHasItem }
public enum ConditionOperator { Equals, NotEquals, GreaterThan, LessThan, GreaterThanOrEquals, LessThanOrEquals }

[Serializable]
public class DialogueNode
{
    public string id;
    public string name;
    public NodeType nodeType = NodeType.Dialogue;
    public EndNodeType endNodeType = EndNodeType.Normal;
    [TextArea(3, 5)] public string dialogueText;
    public string nextDialogueNodeId; // for simple linear dialogues

    // Action Node specific
    public ActionType actionType;
    public string actionId;
    public int actionValue;

    // Condition Node specific
    public ConditionType conditionType;
    public ConditionOperator conditionOperator;
    public string conditionId;
    public int conditionValue;
    public string conditionTrueNodeId;
    public string conditionFalseNodeId;

    public List<DialogueChoice> choices = new();

    [HideInInspector] public Vector2 position; // Added to save graph node positions
}

[CreateAssetMenu(menuName = "Dialogue/Conversation")]
public class DialogueSO : ScriptableObject
{
    public string startNodeId;
    public List<DialogueNode> nodes = new();
}