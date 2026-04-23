using Luci;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    public enum DialogueType
    {
        Fallout, // options appear after dialogue text - no dialogue seen during options
        Persona // options appear after dialogue text - dialogue text is visible during options
    }
    public DialogueType dialogueType = DialogueType.Fallout;

    [Header("Core")]
    [Tooltip("The DialogueRunner that controls the conversation logic.")]
    public DialogueRunner dialogueRunner;

    [Header("UI")]
    public GameObject dialogueUI;
    public GameObject playerHUD;
    public TMP_Text dialogueText, characterName;
    public Transform optionsParentFallout, optionsParentPersona;
    public GameObject optionButtonPrefabFallout, optionButtonPrefabPersona;

    [Header("Player Control")]
    public FirstPersonController playerController;

    private Transform characterHeadPos;

    // internal state
    private readonly List<Button> activeOptionButtons = new List<Button>();
    private readonly List<GameObject> spawnedOptionObjects = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        if (dialogueRunner == null)
        {
            dialogueRunner = FindAnyObjectByType<DialogueRunner>();
            if (dialogueRunner == null)
            {
                Debug.LogError("DialogueManager could not find a DialogueRunner in the scene!");
                enabled = false;
                return;
            }
        }

        // Subscribe to the runner's events
        dialogueRunner.onDialogueNodeStart.AddListener(HandleNodeStart);
        dialogueRunner.onOptionsReady.AddListener(HandleOptionsReady);
        dialogueRunner.onDialogueEnd.AddListener(HandleDialogueEnd);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;

        // Unsubscribe from events
        if (dialogueRunner != null)
        {
            dialogueRunner.onDialogueNodeStart.RemoveListener(HandleNodeStart);
            dialogueRunner.onOptionsReady.RemoveListener(HandleOptionsReady);
            dialogueRunner.onDialogueEnd.RemoveListener(HandleDialogueEnd);
        }
    }

    public void BeginDialogue(CharacterData chardata, Transform characterheadpos)
    {
        characterHeadPos = characterheadpos;

        // --- UI & Player Setup ---
        if (dialogueUI != null) dialogueUI.SetActive(true);
        if (playerHUD != null) playerHUD.SetActive(false);

        if (playerController != null)
        {
            playerController.ToggleDisableCameraHybrid(true, characterHeadPos);
            playerController.ToggleDisableMovement(true);
        }
        Cursor.lockState = CursorLockMode.Confined;

        // --- Start Logic ---
        dialogueRunner.BeginDialogue(chardata);
    }

    private void HandleNodeStart(DialogueNode node, CharacterData characterData)
    {
        ClearOptionsImmediate();

        if (dialogueText != null)
            dialogueText.text = node.dialogueText ?? "";
        if (characterName != null)
            characterName.text = characterData?.characterName ?? "";
    }

    private void HandleOptionsReady(List<DialogueChoice> choices)
    {
        ClearOptionsImmediate();

        Transform optionsParent;
        GameObject optionPrefab;

        if (dialogueType == DialogueType.Fallout)
        {
            if (dialogueText != null) dialogueText.text = "";
            optionsParent = optionsParentFallout;
            optionPrefab = optionButtonPrefabFallout;
        }
        else // Persona
        {
            optionsParent = optionsParentPersona;
            optionPrefab = optionButtonPrefabPersona;
        }

        if (optionsParent == null || optionPrefab == null)
        {
            Debug.LogWarning("Options parent or prefab is missing for the current dialogue type.");
            return;
        }

        foreach (var choice in choices)
        {
            var capturedChoice = choice; // Capture for closure
            GameObject optionButtonObj = Instantiate(optionPrefab, optionsParent);
            spawnedOptionObjects.Add(optionButtonObj);

            var tmp = optionButtonObj.GetComponentInChildren<TMP_Text>();
            if (tmp != null) tmp.text = choice.text;

            var btn = optionButtonObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnOptionSelected(capturedChoice));
                activeOptionButtons.Add(btn);
            }
        }
    }

    private void OnOptionSelected(DialogueChoice choice)
    {
        // Tell the runner which option was chosen
        dialogueRunner.SelectOption(choice);
    }

    private void HandleDialogueEnd()
    {
        if (playerController != null)
        {
            playerController.ToggleDisableCameraHybrid(false, null);
            playerController.ToggleDisableMovement(false);
        }

        if (characterName != null) characterName.text = "";
        if (dialogueText != null) dialogueText.text = "";

        Cursor.lockState = CursorLockMode.Locked;

        ClearOptionsImmediate();

        if (dialogueUI != null) dialogueUI.SetActive(false);
        if (playerHUD != null) playerHUD.SetActive(true);
    }

    private void ClearOptionsImmediate()
    {
        foreach (var btn in activeOptionButtons)
        {
            if (btn != null)
                btn.onClick.RemoveAllListeners();
        }
        activeOptionButtons.Clear();

        foreach (var go in spawnedOptionObjects)
        {
            if (go != null)
                Destroy(go);
        }
        spawnedOptionObjects.Clear();
    }
}