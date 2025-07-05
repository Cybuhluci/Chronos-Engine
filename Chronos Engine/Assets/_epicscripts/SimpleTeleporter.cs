using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleTeleporter : MonoBehaviour
{
    public enum TYPE { location, scene }
    public TYPE type;
    public string TeleportLocat;
    public string TeleportScene;

    public StageManager stageManager;

    private void Start()
    {
        stageManager = FindObjectOfType<StageManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (type == TYPE.scene)
        {
            stageManager.LoadMiscScene(TeleportScene);
        }
    }
}
