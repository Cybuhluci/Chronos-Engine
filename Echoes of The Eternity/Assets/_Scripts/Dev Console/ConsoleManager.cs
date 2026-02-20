using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ConsoleManager : MonoBehaviour
{
    public TMP_InputField inputField;      // Assign in inspector
    public TMP_Text outputText;            // Assign in inspector
    public Button submitButton;            // Assign in inspector
    public PlayerInput playerInput;        // Assign in inspector

    public GameObject developerconsole;
    public bool consoleActive => developerconsole.activeSelf;

    public List<string> commandHistory = new List<string>();
    private int historyIndex = -1;
    private const int maxHistory = 5;
    private bool upPressedLastFrame = false;
    private bool downPressedLastFrame = false;

    public void ToggleConsole()
    {
        developerconsole.SetActive(!developerconsole.activeSelf);
        if (developerconsole.activeSelf)
        {
            inputField.ActivateInputField();
        }
    }

    void Start()
    {
        inputField.onSubmit.AddListener(OnCommandSubmitted);
        submitButton.onClick.AddListener(OnSubmitButtonClicked);
    }

    void OnDestroy()
    {
        inputField.onSubmit.RemoveListener(OnCommandSubmitted);
        submitButton.onClick.RemoveListener(OnSubmitButtonClicked);
    }

    void Update()
    {
        if (playerInput != null && playerInput.actions["submit"] != null &&
            playerInput.actions["submit"].WasPerformedThisFrame())
        {
            SubmitInputField();
        }

        // Command history navigation
        if (playerInput != null && playerInput.actions["ArrowKeys"] != null)
        {
            Vector2 arrow = playerInput.actions["ArrowKeys"].ReadValue<Vector2>();
            // Up arrow
            if (arrow.y > 0.5f && !upPressedLastFrame)
            {
                if (commandHistory.Count > 0 && historyIndex > 0)
                {
                    historyIndex--;
                    inputField.text = commandHistory[historyIndex];
                    inputField.caretPosition = inputField.text.Length;
                }
                else if (commandHistory.Count > 0 && historyIndex == -1)
                {
                    historyIndex = commandHistory.Count - 1;
                    inputField.text = commandHistory[historyIndex];
                    inputField.caretPosition = inputField.text.Length;
                }
                upPressedLastFrame = true;
            }
            else if (arrow.y <= 0.5f)
            {
                upPressedLastFrame = false;
            }
            // Down arrow
            if (arrow.y < -0.5f && !downPressedLastFrame)
            {
                if (commandHistory.Count > 0 && historyIndex < commandHistory.Count - 1 && historyIndex != -1)
                {
                    historyIndex++;
                    inputField.text = commandHistory[historyIndex];
                    inputField.caretPosition = inputField.text.Length;
                }
                else if (historyIndex == commandHistory.Count - 1)
                {
                    historyIndex = commandHistory.Count;
                    inputField.text = "";
                }
                downPressedLastFrame = true;
            }
            else if (arrow.y >= -0.5f)
            {
                downPressedLastFrame = false;
            }
        }
    }

    private void OnSubmitButtonClicked()
    {
        SubmitInputField();
    }

    private void SubmitInputField()
    {
        OnCommandSubmitted(inputField.text);
    }

    private void OnCommandSubmitted(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return;

        // Add to history (avoid duplicates in a row)
        if (commandHistory.Count == 0 || commandHistory[commandHistory.Count - 1] != input)
            commandHistory.Add(input);
        if (commandHistory.Count > maxHistory)
            commandHistory.RemoveAt(0);
        historyIndex = commandHistory.Count;

        bool success = CommandRegistry.Execute(input);
        if (success)
        {
            outputText.text += $"\n> {input}";
        }
        else
        {
            outputText.text += $"\nUnknown command: {input}";
        }
        inputField.text = "";
        inputField.ActivateInputField();
    }
}
