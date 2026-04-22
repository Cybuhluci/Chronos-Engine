using Luci;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    // ways to make this even better:
    // create a dialogue system that can be used in multiple games, with a dialogue graph editor and a way to save/load dialogue states - this way there isnt hundreds of indiviual scriptable objects with one line of text in them.
    // make the dialogue manager both a way to update the ui, and also a way to read given dialogue graphs.

    public static DialogueManager Instance { get; private set; }

    public enum DialogueType
    {
        Fallout, // options appear after dialogue text - no dialogue seen during options
        Persona // options appear after dialogue text - dialogue text is visible during options
    }
    public DialogueType dialogueType = DialogueType.Fallout;

    [Header("Inputs")]
    public PlayerInput playerInput;
    [Tooltip("Name of the action to use to advance dialogue/skip wait (case sensitive). Leave empty to disable.")]
    public string advanceActionName = "Fire";

    [Header("UI")]
    public GameObject dialogueUI;
    public TMP_Text dialogueText, characterName;
    public Transform optionsParent;
    public GameObject optionButtonPrefab;

    [Header("Player control")]
    public FirstPersonController playerController;

    [Header("Timing")]
    [Tooltip("Delay before showing options (seconds)")]
    public float initialDialogDelay = 2f;

    private CharacterData currentCharacterData;
    private Transform characterHeadPos;

    // internal state
    private Coroutine waitCoroutine;
    private readonly List<Button> activeOptionButtons = new List<Button>();
    private readonly List<GameObject> spawnedOptionObjects = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void BeginDialogue(CharacterData chardata, Transform characterheadpos)
    {
        // Stop any previous wait coroutine (if a new dialogue is started quickly)
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }

        characterHeadPos = characterheadpos;

        ClearOptionsImmediate();

        if (dialogueUI != null)
            dialogueUI.SetActive(true);

        if (playerController != null)
        {
            playerController.ToggleDisableCameraHybrid(true, characterHeadPos);
            playerController.ToggleDisableMovement(true);
        }

        Cursor.lockState = CursorLockMode.Confined;

        currentCharacterData = chardata;

        if (characterName != null)
            characterName.text = chardata?.characterName ?? "";

        if (dialogueText != null)
            dialogueText.text = chardata?.temporaryDialogue ?? "";

        // wait for player input or configured delay, then show options
        waitCoroutine = StartCoroutine(WaitForPlayerInputOrTime());
    }

    IEnumerator WaitForPlayerInputOrTime()
    {
        float timer = 0f;
        InputAction advanceAction = null;
        if (!string.IsNullOrEmpty(advanceActionName) && playerInput != null)
        {
            try
            {
                advanceAction = playerInput.actions[advanceActionName];
            }
            catch
            {
                advanceAction = null;
            }
        }

        while (timer < initialDialogDelay)
        {
            if (advanceAction != null && advanceAction.WasPerformedThisFrame())
                break;

            timer += Time.deltaTime;
            yield return null;
        }

        waitCoroutine = null;
        ShowOptions();
    }

    private void ShowOptions()
    {
        ClearOptionsImmediate();

        if (dialogueType == DialogueType.Fallout)
        {
            if (dialogueText != null)
                dialogueText.text = ""; // clear dialogue text when showing options
        }

        if (currentCharacterData == null || currentCharacterData.temporaryOptions == null || optionsParent == null || optionButtonPrefab == null)
            return;

        foreach (var opt in currentCharacterData.temporaryOptions)
        {
            var optionText = opt; // capture for closure
            GameObject optionButtonObj = Instantiate(optionButtonPrefab, optionsParent);
            spawnedOptionObjects.Add(optionButtonObj);

            var tmp = optionButtonObj.GetComponentInChildren<TMP_Text>();
            if (tmp != null) tmp.text = optionText;

            var btn = optionButtonObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnOptionSelected(optionText));
                activeOptionButtons.Add(btn);
            }
        }
    }

    private void OnOptionSelected(string option)
    {
        Debug.Log("Selected option: " + option);
        // handle the option choice here (advance dialogue, call actions, etc.)
        EndDialogue();
    }

    public void EndDialogue()
    {
        // stop waiting coroutine if still running
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }

        if (playerController != null)
        {
            playerController.ToggleDisableCameraHybrid(false, null);
            playerController.ToggleDisableMovement(false);
        }

        if (characterName != null) characterName.text = "";
        if (dialogueText != null) dialogueText.text = "";

        Cursor.lockState = CursorLockMode.Locked;

        ClearOptionsImmediate();

        if (dialogueUI != null)
            dialogueUI.SetActive(false);
    }

    // explicit cleanup for option buttons (removes listeners and destroys objects)
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

        // fallback: still ensure we remove any leftover children
        if (optionsParent != null)
        {
            for (int i = optionsParent.childCount - 1; i >= 0; --i)
            {
                var child = optionsParent.GetChild(i);
                if (child != null)
                    Destroy(child.gameObject);
            }
        }
    }
}