using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }
    public Transform notificationParent; // has vertical layout group and content size fitter components
    public GameObject notificationPrefab;

    private int maxNotifications = 3;

    // runtime list of active notification instances, ordered newest first (index 0)
    private readonly System.Collections.Generic.List<GameObject> _activeNotifications = new System.Collections.Generic.List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ShowNotification(NotificationSO NotifSO)
    {
        // instantiate at parent
        GameObject notification = Instantiate(notificationPrefab, notificationParent);
        // ensure newest notifications appear at top by setting as first sibling
        notification.transform.SetSiblingIndex(0);

        // add to active list and enforce limit
        _activeNotifications.Insert(0, notification);
        if (_activeNotifications.Count > maxNotifications)
        {
            // remove oldest (last in list)
            var oldest = _activeNotifications[_activeNotifications.Count - 1];
            _activeNotifications.RemoveAt(_activeNotifications.Count - 1);
            if (oldest != null)
                Destroy(oldest);
        }
        Notification notificationScript = notification.GetComponent<Notification>();
        if (notificationScript != null)
        {
            notificationScript.SetMessage(NotifSO.message);
            notificationScript.SetImage(NotifSO.sprite); // Set to null or a default image if needed
        }
    }
}
