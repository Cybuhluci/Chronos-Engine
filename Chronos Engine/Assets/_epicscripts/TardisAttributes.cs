using UnityEngine;

public class TardisAttributes : MonoBehaviour
{
    private const string LockKey = "TardisLocked";  // Key for PlayerPrefs

    public static TardisAttributes Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Check if the TARDIS is locked
    public bool IsLocked()
    {
        return PlayerPrefs.GetInt(LockKey, 1) == 1;  // Default to locked (1)
    }

    // Lock the TARDIS
    public void LockTardis()
    {
        PlayerPrefs.SetInt(LockKey, 1);
        PlayerPrefs.Save();
        Debug.Log("TARDIS is now LOCKED.");
    }

    // Unlock the TARDIS
    public void UnlockTardis()
    {
        PlayerPrefs.SetInt(LockKey, 0);
        PlayerPrefs.Save();
        Debug.Log("TARDIS is now UNLOCKED.");
    }
}
