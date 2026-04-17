using UnityEngine;
using UnityEngine.InputSystem;

public class TempNotificationTester : MonoBehaviour
{
    [SerializeField] private NotificationSO[] notiSOs;
    [SerializeField] private PlayerInput playerInput;

    private void Update()
    {
        if (playerInput.actions["Quicknade"].triggered)
        {
            int randomIndex = Random.Range(0, notiSOs.Length);
            if (randomIndex >= 0 && randomIndex < notiSOs.Length)
            {
                NotificationManager.Instance.ShowNotification(notiSOs[randomIndex]);
            }
        }
    }
}
