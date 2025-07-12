using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Crosshair_Colour : MonoBehaviour
{
    [Header("Crosshair Images")]
    [SerializeField] private Image crosshairDefault;
    [SerializeField] private Image crosshairInteractable;
    [SerializeField] private Image crosshairProp;
    [SerializeField] private Image crosshairLongInteract;

    private Image _activeCrosshair;
    private bool _needsUpdate = false;

    // Call this from PlayerLookInteract when the crosshair type changes
    public void SetActiveCrosshair(EInteractionType type)
    {
        switch (type)
        {
            case EInteractionType.Normal:
                _activeCrosshair = crosshairDefault;
                break;
            case EInteractionType.InteractShort:
                _activeCrosshair = crosshairInteractable;
                break;
            case EInteractionType.Prop:
                _activeCrosshair = crosshairProp;
                break;
            case EInteractionType.LongInteract:
                _activeCrosshair = crosshairLongInteract;
                break;
            default:
                _activeCrosshair = crosshairDefault;
                break;
        }
        _needsUpdate = true; // Force update on crosshair change
    }

    void Update()
    {
        // Only update if a crosshair is active and enabled
        if (_activeCrosshair != null && _activeCrosshair.gameObject.activeInHierarchy)
        {
            _needsUpdate = true;
        }
        if (_needsUpdate)
        {
            StartCoroutine(UpdateCrosshairColorNextFrame());
            _needsUpdate = false;
        }
    }

    private IEnumerator UpdateCrosshairColorNextFrame()
    {
        yield return new WaitForEndOfFrame();
        if (_activeCrosshair != null && _activeCrosshair.gameObject.activeInHierarchy)
        {
            Color bgColor = SampleScreenCenterColor();
            _activeCrosshair.color = GetContrastingColor(bgColor);
        }
    }

    // Sample the color at the center of the screen
    private Color SampleScreenCenterColor()
    {
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(Screen.width / 2, Screen.height / 2, 1, 1), 0, 0);
        tex.Apply();
        Color color = tex.GetPixel(0, 0);
        Destroy(tex);
        return color;
    }

    // Returns black or white depending on background brightness
    private Color GetContrastingColor(Color bg)
    {
        float brightness = (bg.r * 299 + bg.g * 587 + bg.b * 114) / 1000f;
        return brightness > 0.5f ? Color.black : Color.white;
    }
}
