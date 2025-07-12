using UnityEngine;
using Luci.Interactions;

public class tardisdoorlockscript : MonoBehaviour, IInteractable
{
    private const string TardisLockKey = "TardisLocked";  // Key for PlayerPrefs
    public string requiredKeyName;

    public void LeftInteract()
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

    public void RightInteract()
    {
        Debug.Log("RightInteract");
    }

    public bool IsLocked()
    {
        return PlayerPrefs.GetInt(TardisLockKey, 1) == 1;  // Default to locked
    }

    public EInteractionType GetInteractionType()
    {
        return EInteractionType.InteractShort; // Returns the enum value for a short interaction
    }
}
