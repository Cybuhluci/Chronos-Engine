using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreen : loadinghints
{
    public string StageName1;
    public string StageName2;
    public TMP_Text StageName1txt;
    public TMP_Text StageName2txt;

    public GameObject spinnyLoadingThing; // The spinning object
    public float lengthOfLoading = 3f;
    public float spinSpeed = 200f; // Rotation speed in degrees per second

    private string levelToLoad;
    private string previousScene;
    public bool over;
    public int endCount;
    public bool stageLoaded;

    public Animator Anim;
    public Camera Cam;

    [Header("Hint System")]
    public TMP_Text hintText; // Assign the UI Text component
    public TMP_Text hintNumberText; // New text field to show hint number
    private int currentHintIndex = 0; // Track the current hint

    void Start()
    {
        if (PlayerPrefs.GetInt("IsStageLoading") == 1 && Anim != null)
        {
            Anim.SetBool("loaded", false);
        }

        if (StageName1txt != null)
        {
            StageName1txt.text = StageName1;
            StageName2txt.text = StageName2;
        }

        levelToLoad = PlayerPrefs.GetString("NextScene", "DefaultScene");
        previousScene = PlayerPrefs.GetString("PreviousScene", "DefaultScene");

        if (levelToLoad == "DefaultScene")
        {
            //Debug.LogError("ERROR: No scene name found! Did you set 'NextScene' before loading?");
            return;
        }

        //Debug.Log($"Loading screen active. Loading: {levelToLoad}, Unloading: {previousScene}");

        if (PlayerPrefs.GetInt("IsStageLoading", 0) == 2)
        {
            ShowRandomHint();
        }

        StartCoroutine(LoadLevelAsync());
    }

    private IEnumerator LoadLevelAsync()
    {
        SceneManager.UnloadSceneAsync(previousScene);

        yield return new WaitForSeconds(lengthOfLoading); // Simulate loading delay

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelToLoad, LoadSceneMode.Additive);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f) // Wait until almost ready
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;
        stageLoaded = true;
    }

    void Update()
    {
        if (spinnyLoadingThing != null)
        {
            spinnyLoadingThing.transform.Rotate(Vector3.forward, -spinSpeed * Time.deltaTime);
        }

        // Change hint when clicking the left mouse button
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            ShowNextHint();
        }
    }

    void FixedUpdate()
    {
        if (stageLoaded)
        {
            over = true;
        }

        if (over)
        {
            endCount++;

            if (endCount == 30)
            {
                if (PlayerPrefs.GetInt("IsStageLoading") == 1)
                {
                    Anim.SetBool("loaded", true);
                }
            }
            else if (endCount > 100)
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(levelToLoad));
                //Debug.Log("CLOSED LOADING SCREEN");
                SceneManager.UnloadSceneAsync("loadingScene");
                PlayerPrefs.SetInt("inloadingscreen", 0);
            }
        }
    }

    void ShowRandomHint()
    {
        if (hints.Length == 0) return;

        // Pick a random hint index
        currentHintIndex = Random.Range(0, hints.Length);

        // Display the hint
        UpdateHintText();
    }

    void ShowNextHint()
    {
        if (hints.Length == 0) return;

        // Move to the next hint, loop back if at the end
        currentHintIndex = (currentHintIndex + 1) % hints.Length;

        // Display the hint
        UpdateHintText();
    }

    void UpdateHintText()
    {
        hintText.text = hints[currentHintIndex];

        if (hintNumberText != null)
        {
            hintNumberText.text = "TARDIS DATABANK ENTRY #" + (currentHintIndex + 1);
        }
    }
}