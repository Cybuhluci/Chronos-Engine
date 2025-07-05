using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class forceSceneChange : MonoBehaviour
{
    public string Scene;
    public void SceneChange()
    {
        SceneManager.LoadScene(Scene);
    }
}
