using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ConsoleManager : MonoBehaviour
{
    public static ConsoleManager instance;

    public GameObject gameHUD;              // Assign in inspector (optional, for toggling HUD visibility)

    public TMP_InputField inputField;      // Assign in inspector
    public TMP_Text outputText;            // Assign in inspector
    public Button submitButton;            // Assign in inspector
    public PlayerInput playerInput;        // Assign in inspector

    [Header("Output Auto-Resize")]
    [Tooltip("Optional RectTransform to resize. If null the script will use outputText.rectTransform.")]
    public RectTransform outputTextRect;
    [Tooltip("Optional ScrollRect to auto-scroll when new text is added.")]
    public ScrollRect outputScrollRect;
    [Tooltip("Minimum height (px) for the output area")] public float outputMinHeight = 100f;

    public GameObject developerconsole;
    public bool consoleActive => developerconsole.activeSelf;

    public List<string> commandHistory = new List<string>();
    private int historyIndex = -1;
    private const int maxHistory = 5;
    private bool upPressedLastFrame = false;
    private bool downPressedLastFrame = false;

    private CursorLockMode cursorLockState;

    public void ToggleConsole()
    {
        cursorLockState = Cursor.lockState;
        Cursor.lockState = developerconsole.activeSelf ? cursorLockState : CursorLockMode.None;

        gameHUD.SetActive(developerconsole.activeSelf);

        developerconsole.SetActive(!developerconsole.activeSelf);
        if (developerconsole.activeSelf)
        {
            inputField.ActivateInputField();
        }
    }

    void Start()
    {
        instance = this;

        inputField.onSubmit.AddListener(OnCommandSubmitted);
        submitButton.onClick.AddListener(OnSubmitButtonClicked);
        // Ensure initial size is correct
        AdjustOutputHeight();
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
            outputText.text += $"] {input}\n";
        }
        else
        {
            outputText.text += $"Unknown command: {input}\n";
        }
        // Resize output area to fit the new contents
        AdjustOutputHeight();
        // Auto-scroll to bottom if a ScrollRect was provided
        if (outputScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            outputScrollRect.verticalNormalizedPosition = 0f;
            Canvas.ForceUpdateCanvases();
        }
        inputField.text = "";
        inputField.ActivateInputField();
    }

    // Resize the output text RectTransform to match TMP preferred height (clamped).
    private void AdjustOutputHeight()
    {
        if (outputText == null) return;

        RectTransform rt = outputTextRect != null ? outputTextRect : outputText.rectTransform;

        // Force mesh update so preferred values are accurate
        outputText.ForceMeshUpdate();

        // Use GetPreferredValues with current width to compute required height
        float width = rt.rect.width;
        Vector2 preferred = outputText.GetPreferredValues(outputText.text, width, 0f);
        float targetHeight = preferred.y;

        // Enforce minimum and apply (no maximum clamp to allow long backlog)
        targetHeight = Mathf.Max(targetHeight, outputMinHeight);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);

        // Ensure layout updates immediately (useful if parent uses LayoutGroup)
        Canvas.ForceUpdateCanvases();
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    // Public helper to append a line to the console output and update layout/scroll
    public void AppendOutput(string line)
    {
        if (outputText == null) return;
        outputText.text += line + "\n";
        AdjustOutputHeight();
        if (outputScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            outputScrollRect.verticalNormalizedPosition = 0f;
            Canvas.ForceUpdateCanvases();
        }
    }

    // Public helper to clear the console output
    public void ClearOutput()
    {
        if (outputText == null) return;
        outputText.text = string.Empty;
        AdjustOutputHeight();
        if (outputScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            outputScrollRect.verticalNormalizedPosition = 0f;
            Canvas.ForceUpdateCanvases();
        }
    }
}
