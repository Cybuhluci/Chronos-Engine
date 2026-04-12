using UnityEngine;

[CreateAssetMenu(menuName = "UI/UITheme")]
public class UITheme : ScriptableObject
{
    public Color Primary;       // main tint (bars, icons, borders)
    public Color PrimaryDark;   // tracks, backplates
    public Color Highlight;     // filled bar / selection
    public Color Alert;         // danger (reserve use)
    public Color TextPrimary;   // main tint for text, for use on dark backgrounds
    public Color TextPrimaryDark; // secondary tint for text, for use on light backgrounds


    [Header("Optional settings")]
    [Range(0f, 1f)] public float SubtitleBackdropAlpha = 0.35f;
}