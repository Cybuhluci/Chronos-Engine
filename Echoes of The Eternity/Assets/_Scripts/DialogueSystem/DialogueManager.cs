using Luci;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    public PlayerInput playerInput;
    public GameObject dialogueUI;
    public TMP_Text dialogueText, characterName;
    public FirstPersonController playerController;
    public Transform optionsParent;
    public GameObject optionButtonPrefab;
    private CharacterData currentCharacterData;
    public Transform cameraLookAtTransform;
    public Transform characterHeadPos;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public void beginDialogue(CharacterData chardata, Transform characterheadpos)
    {
        characterHeadPos = characterheadpos;

        // clear previous options
        foreach (Transform child in optionsParent)
        {
            Destroy(child.gameObject);
        }
        dialogueUI.SetActive(true);
        playerController.ToggleDisableCameraHybrid(true, characterHeadPos);
        playerController.ToggleDisableMovement(true);

        Cursor.lockState = CursorLockMode.Confined;

        // store current character so ShowOptions can access its data
        currentCharacterData = chardata;

        characterName.text = chardata.characterName;
        dialogueText.text = chardata.temporaryDialogue;
        // wait for player input or 2 seconds, then show options
        StartCoroutine(WaitForPlayerInputOrTime());
    }

    IEnumerator WaitForPlayerInputOrTime()
    {
        float timer = 0f;
        while (timer < 2f)
        {
            if (playerInput.actions["Fire"].WasPerformedThisFrame()) // Example input
                break;
            timer += Time.deltaTime;
            yield return null;
        }
        ShowOptions();
    }

    private void ShowOptions()
    {
        // clear previous options
        foreach (Transform child in optionsParent)
        {
            Destroy(child.gameObject);
        }

        dialogueText.text = ""; // clear dialogue text when showing options

        // instantiate options buttons and set their text to the options in the character data
        if (currentCharacterData == null || currentCharacterData.temporaryOptions == null) return;
        foreach (var opt in currentCharacterData.temporaryOptions)
        {
            var optionText = opt; // capture for closure
            GameObject optionButton = Instantiate(optionButtonPrefab, optionsParent);
            var tmp = optionButton.GetComponentInChildren<TMP_Text>();
            if (tmp != null) tmp.text = optionText;
            var btn = optionButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnOptionSelected(optionText));
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
        playerController.ToggleDisableCameraHybrid(false, null);
        playerController.ToggleDisableMovement(false);
        characterName.text = "";
        dialogueText.text = "";

        Cursor.lockState = CursorLockMode.Locked;

        // destroy options buttons
        foreach (Transform child in optionsParent)
        {
            Destroy(child.gameObject);
        }
        dialogueUI.SetActive(false);
    }
}
