using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// Events to communicate with the UI layer (DialogueManager)
[Serializable] public class OnDialogueNodeStart : UnityEvent<DialogueNode, CharacterData> { }
[Serializable] public class OnOptionsReady : UnityEvent<List<DialogueChoice>> { }
[Serializable] public class OnDialogueEndEvent : UnityEvent<EndNodeType> { }

public class DialogueRunner : MonoBehaviour
{
    [Header("Inputs")]
    [Tooltip("Name of the action to use to advance dialogue/skip wait (case sensitive). Leave empty to disable.")]
    public string advanceActionName = "Fire";
    public PlayerInput playerInput;

    [Header("Timing")]
    [Tooltip("Delay before showing options (seconds)")]
    public float initialDialogDelay = 2f;

    // --- Events ---
    public OnDialogueNodeStart onDialogueNodeStart;
    public OnOptionsReady onOptionsReady;
    public OnDialogueEndEvent onDialogueEnd;

    // --- Internal State ---
    private DialogueSO currentDialogueSO;
    private CharacterData currentCharacterData;
    private DialogueNode currentDialogueNode;
    private string currentNodeId;
    private Dictionary<string, DialogueNode> nodeLookup = new Dictionary<string, DialogueNode>();
    private Coroutine waitCoroutine;
    private InputAction advanceAction;
    private bool advancePressed;

    public bool IsDialogueActive { get; private set; }

    private void OnDestroy()
    {
        UnregisterAdvanceAction();
    }

    public void BeginDialogue(CharacterData characterData)
    {
        if (characterData == null || characterData.dialogue == null)
        {
            Debug.LogWarning("Character data or dialogue SO is null.");
            return;
        }

        IsDialogueActive = true;
        currentCharacterData = characterData;
        currentDialogueSO = characterData.dialogue;
        currentNodeId = currentDialogueSO.startNodeId;

        nodeLookup.Clear();
        foreach (var node in currentDialogueSO.nodes)
        {
            nodeLookup[node.id] = node;
        }

        RegisterAdvanceAction();
        ShowDialogue(currentNodeId);
    }

    public void SelectOption(DialogueChoice choice)
    {
        if (!IsDialogueActive) return;

        if (choice.endsConversation)
        {
            EndDialogue();
        }
        else
        {
            ShowDialogue(choice.targetNodeId);
        }
    }

    private void ShowDialogue(string nodeId)
    {
        if (currentDialogueSO == null)
        {
            EndDialogue();
            return;
        }

        if (!nodeLookup.TryGetValue(nodeId, out currentDialogueNode))
        {
            Debug.LogWarning($"Dialogue node '{nodeId}' not found. Ending dialogue.");
            EndDialogue();
            return;
        }

        currentNodeId = nodeId;
        advancePressed = false;

        // Process based on type
        switch (currentDialogueNode.nodeType)
        {
            case NodeType.Start:
                // Instantly proxy to the next node
                ShowDialogue(currentDialogueNode.nextDialogueNodeId);
                break;

            case NodeType.Dialogue:
                onDialogueNodeStart.Invoke(currentDialogueNode, currentCharacterData);
                if (waitCoroutine != null) StopCoroutine(waitCoroutine);
                waitCoroutine = StartCoroutine(WaitForInputOrTime());
                break;

            case NodeType.Choice:
                onDialogueNodeStart.Invoke(currentDialogueNode, currentCharacterData); // show Context string
                onOptionsReady.Invoke(currentDialogueNode.choices);
                break;

            case NodeType.Action:
                Debug.Log($"Dialogue ActionTriggered: {currentDialogueNode.actionType} | ID: '{currentDialogueNode.actionId}' | Value: {currentDialogueNode.actionValue}");
                
                switch (currentDialogueNode.actionType)
                {
                    case ActionType.Event:
                        EventManager.Instance.SetEvent(currentDialogueNode.actionId, currentDialogueNode.actionValue);
                        break;
                    case ActionType.AddItem:
                        InventoryManager.Instance.AddItemByID(currentDialogueNode.actionId, currentDialogueNode.actionValue); // Example
                        break;
                    case ActionType.RemoveItem:
                        InventoryManager.Instance.RemoveItemByID(currentDialogueNode.actionId, currentDialogueNode.actionValue); // Example
                        break;
                    case ActionType.AddPositiveKarma:
                        FactionManager.Instance.AddKarma(currentDialogueNode.actionId, currentDialogueNode.actionValue, 0); // Example
                        break;
                    case ActionType.AddNegativeKarma:
                        FactionManager.Instance.AddKarma(currentDialogueNode.actionId, 0, currentDialogueNode.actionValue); // Example
                        break;
                }
                
                // Immediately transition to the next node (instantly proxies like Start node does)
                ShowDialogue(currentDialogueNode.nextDialogueNodeId);
                break;

            case NodeType.Condition:
                bool conditionResult = EvaluateCondition(currentDialogueNode);
                Debug.Log($"Dialogue Condition Triggered: {currentDialogueNode.conditionType} | ID: '{currentDialogueNode.conditionId}'. Evaluated to: {conditionResult}");
                string nextNode = conditionResult ? currentDialogueNode.conditionTrueNodeId : currentDialogueNode.conditionFalseNodeId;
                ShowDialogue(nextNode);
                break;

            case NodeType.End:
                EndDialogue(currentDialogueNode.endNodeType);
                break;
        }
    }

    private IEnumerator WaitForInputOrTime()
    {
        float timer = 0f;
        while (timer < initialDialogDelay)
        {
            if (advancePressed) break;
            timer += Time.deltaTime;
            yield return null;
        }

        advancePressed = false;
        waitCoroutine = null;

        // In the new system, choices are on Choice nodes. For linear Dialogue nodes, we just go next.
        if (!string.IsNullOrEmpty(currentDialogueNode.nextDialogueNodeId))
        {
            ShowDialogue(currentDialogueNode.nextDialogueNodeId);
        }
        else
        {
            EndDialogue(EndNodeType.Normal); // Safety fallback
        }
    }

    private void EndDialogue(EndNodeType endType = EndNodeType.Normal)
    {
        IsDialogueActive = false;
        UnregisterAdvanceAction();
        nodeLookup.Clear();
        onDialogueEnd.Invoke(endType);
    }

    private bool EvaluateCondition(DialogueNode node)
    {
        int currentValue = 0;

        // Populate currentValue based on type
        switch (node.conditionType)
        {
            case ConditionType.Event:
                currentValue = EventManager.Instance.GetEventValue(node.conditionId);
                break;
            case ConditionType.FactionPositiveKarma:
                currentValue = FactionManager.Instance.GetPositiveKarma(node.conditionId);
                break;
            case ConditionType.FactionNegativeKarma:
                currentValue = FactionManager.Instance.GetNegativeKarma(node.conditionId);
                break;
            case ConditionType.PlayerHasItem:
                currentValue = InventoryManager.Instance.GetItemCount(node.conditionId);
                break;
        }

        // Compare using Operator
        switch (node.conditionOperator)
        {
            case ConditionOperator.Equals: return currentValue == node.conditionValue;
            case ConditionOperator.NotEquals: return currentValue != node.conditionValue;
            case ConditionOperator.GreaterThan: return currentValue > node.conditionValue;
            case ConditionOperator.LessThan: return currentValue < node.conditionValue;
            case ConditionOperator.GreaterThanOrEquals: return currentValue >= node.conditionValue;
            case ConditionOperator.LessThanOrEquals: return currentValue <= node.conditionValue;
            default: return false;
        }
    }

    #region Input Handling
    private void RegisterAdvanceAction()
    {
        UnregisterAdvanceAction();
        advancePressed = false;
        if (string.IsNullOrEmpty(advanceActionName) || playerInput == null) return;
        try
        {
            advanceAction = playerInput.actions[advanceActionName];
            if (advanceAction != null)
                advanceAction.performed += OnAdvancePerformed;
        }
        catch { advanceAction = null; }
    }

    private void UnregisterAdvanceAction()
    {
        if (advanceAction != null)
        {
            advanceAction.performed -= OnAdvancePerformed;
            advanceAction = null;
        }
        advancePressed = false;
    }

    private void OnAdvancePerformed(InputAction.CallbackContext ctx)
    {
        advancePressed = true;
    }
    #endregion
}
