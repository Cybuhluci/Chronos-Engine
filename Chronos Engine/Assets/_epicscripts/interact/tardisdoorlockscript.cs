using UnityEngine;

public class tardisdoorlockscript : MonoBehaviour, IInteractable
{
    private const string TardisLockKey = "TardisLocked";  // Key for PlayerPrefs
    public string requiredKeyName;

    public void Interact()
    {
        if (InventoryManager.Instance.HasItem(requiredKeyName) || PlayerPrefs.GetInt("HasTardisKey", 0) == 1)  // Check if player has the key
        {
            bool isLocked = !IsLocked();  // Flip the lock state
            PlayerPrefs.SetInt(TardisLockKey, isLocked ? 1 : 0);
            PlayerPrefs.Save();

            Debug.Log(isLocked ? "TARDIS is now LOCKED." : "TARDIS is now UNLOCKED.");
        }
        else
        {
            Debug.Log("Can't open door without TARDIS key");
        }
    }

    public bool IsLocked()
    {
        return PlayerPrefs.GetInt(TardisLockKey, 1) == 1;  // Default to locked
    }

    public string GetInteractionType()
    {
        return "Tardis Lock";
    }
}
