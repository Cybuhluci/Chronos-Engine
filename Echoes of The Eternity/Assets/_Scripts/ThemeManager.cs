using UnityEngine;
using System.Collections;

public class ThemeManager : MonoBehaviour
{
    public static ThemeManager Instance { get; private set; }
    public UITheme CurrentTheme;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Immediate apply or lerped apply (duration > 0)
    public void ApplyTheme(UITheme theme, float duration = 0f)
    {
        if (theme == null) return;

        if (duration <= 0f)
        {
            CurrentTheme = theme;
            PushThemeToGlobals(theme);
        }
        else
        {
            StartCoroutine(LerpTheme(theme, duration));
        }
    }

    void PushThemeToGlobals(UITheme theme)
    {
        // Set shader/global colors (if you use a UI shader that samples global props)
        Shader.SetGlobalColor("_UIPrimary", theme.Primary);
        Shader.SetGlobalColor("_UIDark", theme.PrimaryDark);
        Shader.SetGlobalColor("_UIHighlight", theme.Highlight);
        Shader.SetGlobalColor("_UIAlert", theme.Alert);
        Shader.SetGlobalColor("_UITextPrimary", theme.TextPrimary);
    }

    IEnumerator LerpTheme(UITheme target, float duration)
    {
        UITheme startTheme = CurrentTheme;
        if (startTheme == null)
        {
            startTheme = ScriptableObject.CreateInstance<UITheme>();
            startTheme.Primary = target.Primary;
            startTheme.PrimaryDark = target.PrimaryDark;
            startTheme.Highlight = target.Highlight;
            startTheme.Alert = target.Alert;
            startTheme.TextPrimary = target.TextPrimary;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            Shader.SetGlobalColor("_UIPrimary", Color.Lerp(startTheme.Primary, target.Primary, p));
            Shader.SetGlobalColor("_UIDark", Color.Lerp(startTheme.PrimaryDark, target.PrimaryDark, p));
            Shader.SetGlobalColor("_UIHighlight", Color.Lerp(startTheme.Highlight, target.Highlight, p));
            Shader.SetGlobalColor("_UIAlert", Color.Lerp(startTheme.Alert, target.Alert, p));
            Shader.SetGlobalColor("_UITextPrimary", Color.Lerp(startTheme.TextPrimary, target.TextPrimary, p));
            yield return null;
        }

        CurrentTheme = target;
    }
}