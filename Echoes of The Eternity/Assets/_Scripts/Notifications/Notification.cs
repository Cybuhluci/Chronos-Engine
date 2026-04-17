using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Notification : MonoBehaviour
{
    public TMP_Text notificationText;
    public Image notificationImage;
    public float displayDuration = 10f; // Duration to display the notification (default)
    public float fadeDuration = 2f; // how long the fade-out lasts

    public float _timer = 0f;
    private bool _running = false;
    private CanvasGroup _canvasGroup;

    public void SetMessage(string message)
    {
        if (notificationText != null)
        {
            notificationText.text = message;
        }
    }

    public void SetImage(Sprite image)
    {
        if (notificationImage != null)
        {
            notificationImage.sprite = image;
        }
    }

    private void OnEnable()
    {
        StartNotification();
    }

    /// <summary>
    /// Start the notification timer and optionally override durations.
    /// </summary>
    public void StartNotification(float duration = -1f, float fade = -1f)
    {
        if (duration > 0f) displayDuration = duration;
        if (fade >= 0f) fadeDuration = fade;

        // ensure we have a CanvasGroup to control overall alpha
        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        _canvasGroup.alpha = 1f;
        _timer = displayDuration;
        _running = true;
    }

    private void Update()
    {
        if (!_running) return;

        _timer -= Time.deltaTime;

        // start fading when time remaining <= fadeDuration
        if (_timer <= fadeDuration)
        {
            float a = Mathf.Clamp01(Mathf.Max(0f, _timer) / Mathf.Max(0.0001f, fadeDuration));
            if (_canvasGroup != null)
                _canvasGroup.alpha = a;
            else
            {
                // fallback: fade text and image colors
                if (notificationText != null)
                {
                    var c = notificationText.color;
                    c.a = a;
                    notificationText.color = c;
                }
                if (notificationImage != null)
                {
                    var c = notificationImage.color;
                    c.a = a;
                    notificationImage.color = c;
                }
            }
        }

        if (_timer <= 0f)
        {
            _running = false;
            Destroy(gameObject);
        }
    }
}
