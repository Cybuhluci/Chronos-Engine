using UnityEngine;

[CreateAssetMenu(fileName = "NotificationSO", menuName = "Luci/NotificationSO")]
public class NotificationSO : ScriptableObject
{
    [TextArea(1, 5)] public string message;
    public Sprite sprite;
}
