using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class introcutscene : MonoBehaviour
{
    public PlayerInput input;
    public VideoPlayer videoPlayer;

    void Start()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("Video finished playing!");
        SceneManager.LoadScene("mainmenu");
    }

    // Update is called once per frame
    void Update()
    {
        if (input.actions.FindAction("anyandall").WasPressedThisFrame())
        {
            SceneManager.LoadScene("mainmenu");
        }
    }
}
