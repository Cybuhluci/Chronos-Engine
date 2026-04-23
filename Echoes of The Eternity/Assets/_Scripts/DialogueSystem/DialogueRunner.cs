using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// Events to communicate with the UI layer (DialogueManager)
[Serializable] public class OnDialogueNodeStart : UnityEvent<DialogueNode, CharacterData> { }
[Serializable] public class OnOptionsReady : UnityEvent<List<DialogueChoice>> { }

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
    public UnityEvent onDialogueEnd;

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
            Debug.LogWarning($"Dialogue node '{nodeId}' not found in '{currentDialogueSO.name}'. Ending dialogue.");
            EndDialogue();
            return;
        }

        currentNodeId = nodeId;
        advancePressed = false;

        // Notify the UI to display this node's content
        onDialogueNodeStart.Invoke(currentDialogueNode, currentCharacterData);

        if (waitCoroutine != null) StopCoroutine(waitCoroutine);
        waitCoroutine = StartCoroutine(WaitForInputOrTime());
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

        // If the node has choices, present them
        if (currentDialogueNode.choices != null && currentDialogueNode.choices.Count > 0)
        {
            onOptionsReady.Invoke(currentDialogueNode.choices);
        }
        // If there's a next node, automatically advance
        else if (!string.IsNullOrEmpty(currentDialogueNode.nextDialogueNodeId))
        {
            ShowDialogue(currentDialogueNode.nextDialogueNodeId);
        }
        // Otherwise, end the conversation
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        IsDialogueActive = false;
        UnregisterAdvanceAction();
        nodeLookup.Clear();
        onDialogueEnd.Invoke();
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
