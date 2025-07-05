using UnityEngine;

public class BTN_changetardislocation : MonoBehaviour
{
    public tardisdoorscript doorscript;
    public string NewSceneToGoTo;

    public void onbuttonpress()
    {
        doorscript.SceneToGo = NewSceneToGoTo;
        Debug.Log("TARDIS door changed to: " + NewSceneToGoTo);
    }
}