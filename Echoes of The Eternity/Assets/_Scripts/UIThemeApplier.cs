using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach to a GameObject with Image and/or TextMeshProUGUI components.
// It will apply theme colours from ThemeManager.CurrentTheme at runtime or in editor.
[ExecuteAlways]
public class UIThemeApplier : MonoBehaviour
{
    public enum TargetType { Image, TMPText, ImageAndText }
    public TargetType target = TargetType.ImageAndText;

    public bool usePrimary = true;
    public bool useHighlightForFill = false;
    public bool isAlert = false;
    public bool setTextColor = true;

    Image img;
    TMP_Text tmp;

    void OnEnable()
    {
        img = GetComponent<Image>();
        tmp = GetComponent<TMP_Text>();
        Apply();
    }

    void Update()
    {
        Apply();
    }

    public void Apply()
    {
        if (ThemeManager.Instance == null || ThemeManager.Instance.CurrentTheme == null) return;
        var theme = ThemeManager.Instance.CurrentTheme;

        if ((target == TargetType.Image || target == TargetType.ImageAndText) && img != null)
        {
            img.color = usePrimary ? theme.Primary : theme.PrimaryDark;
        }

        if ((target == TargetType.TMPText || target == TargetType.ImageAndText) && tmp != null && setTextColor)
        {
            tmp.color = usePrimary ? theme.TextPrimary : theme.TextPrimaryDark;
        }

        if (isAlert && tmp != null && setTextColor)
        {
            tmp.color = theme.Alert;
        }

        if (isAlert && img != null)
        {
            img.color = theme.Alert;
        }
    }
}