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

    // Conditions
    public string requiredFlag;
    public bool requiredValue;

    // Effects
    public string setFlag;
    public bool setValue;

    public UnityEvent onChosen; // Event to trigger when this choice is selected
}

[Serializable]
public class DialogueNode
{
    public string id;
    public string name;
    [TextArea] public string dialogueText;
    public string nextDialogueNodeId; // for simple linear dialogues

    public List<DialogueChoice> choices = new();
}

[CreateAssetMenu(menuName = "Dialogue/Conversation")]
public class DialogueSO : ScriptableObject
{
    public string startNodeId;
    public List<DialogueNode> nodes = new();
}