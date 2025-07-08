using UnityEngine;
using UnityEngine.SceneManagement;

public class tardisdoorscript : MonoBehaviour, IInteractable
{
    public enum TardisType { FlightMode, SceneInterior, PrefabInterior, SceneInteriorExit, PrefabInteriorExit }
    public TardisType tardisType;

    [Header("REQUIRED")]
    public GameObject playerCharacter;
    public tardisdoorlockscript doorLockScript;

    [Header("TARDIS Flight Mode")]
    //public TardisController flightcontroller;
    public GameObject staticTardis;
    public GameObject playerTardis;
    public bool isFlying = true;
    public bool antiGravsOff = true;

    [Header("TARDIS Scene Interior Mode")]
    public string SceneToGo = "TARDISinterior";
    public string oldScene;
    public StageManager sceneManager;

    [Header("TARDIS Prefab Interior Mode")]
    public GameObject tardisPrefabDoorEntrance;

    private void Start()
    {
        if (sceneManager == null)
        {
            Debug.LogError("SceneManagerScript not found in the scene!");
        }

        if (doorLockScript == null)
        {
            Debug.LogError("TardisDoorLockScript is missing on TARDIS door!");
        }
    }

    private void Update()
    {
        sceneManager = FindFirstObjectByType<StageManager>();

        if (playerTardis != null)
        {
            //isFlying = flightcontroller.isActive;
            //antiGravsOff = flightcontroller.gravityEnabled;
        }
    }

    public void Interact()
    {
        if (doorLockScript != null && doorLockScript.IsLocked())
        {
            Debug.Log("TARDIS door is locked! Cannot enter.");
            return;
        }

        Debug.Log("Entering the TARDIS!");

        switch (tardisType)
        {
            case TardisType.FlightMode:
                ActivateFlightMode();
                break;
            case TardisType.SceneInterior:
                EnterSceneTardis(SceneToGo);
                break;
            case TardisType.PrefabInterior:
                EnterPrefabTardis();
                break;
            case TardisType.PrefabInteriorExit:
                ExitPrefabTardis();
                break;
        }
    }

    public EInteractionType GetInteractionType()
    {
        return EInteractionType.InteractShort; // Returns the enum value for a short interaction
    }

    private void ActivateFlightMode()
    {
        if (!isFlying && antiGravsOff)
        {
            staticTardis.SetActive(false);
            playerTardis.SetActive(true);
            playerCharacter.SetActive(false);
            Debug.Log("TARDIS Flight Mode activated.");
        }
        else
        {
            Debug.Log("Cannot enter while the TARDIS is in motion or anti-gravs are active!");
        }
    }

    private void EnterSceneTardis(string sceneName)
    {
        if (sceneManager != null)
        {
            oldScene = SceneManager.GetActiveScene().name;

            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("ERROR: Scene name is empty or null!");
                return;
            }

            // Save scene names
            PlayerPrefs.SetString("NextScene", sceneName);
            PlayerPrefs.SetString("PreviousScene", oldScene);
            PlayerPrefs.SetInt("IsStageLoading", 2); // 0 = Misc
            PlayerPrefs.Save(); // Ensure data is written

            Debug.Log($"Loading screen opened. Next Scene: {sceneName}, Previous Scene: {SceneManager.GetActiveScene().name}");

            SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);
        }
    }

    private void EnterPrefabTardis()
    {
        if (tardisPrefabDoorEntrance)
        {
            playerCharacter.transform.position = tardisPrefabDoorEntrance.transform.position;
            Debug.Log("Player moved to TARDIS prefab interior.");
        }
        else
        {
            Debug.LogError("TARDIS Prefab Interior Entrance is not set!");
        }
    }

    private void ExitPrefabTardis()
    {
        playerCharacter.transform.position = staticTardis.transform.position + new Vector3(0, 0, 2);
        Debug.Log("Player exited prefab interior and is now outside the TARDIS.");
    }
}
