using UnityEngine;

public class SceneChange : MonoBehaviour
{
    public StageManager StageManager;

    void Start()
    {
        StageManager = FindAnyObjectByType<StageManager>();
    }

    private void Awake()
    {
        StageManager = FindAnyObjectByType<StageManager>();
    }

    public void ChangeScene(string Scene)
    {
        StageManager.LoadMiscScene(Scene);
    }
}
