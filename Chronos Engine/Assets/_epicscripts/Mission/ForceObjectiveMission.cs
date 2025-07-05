using UnityEngine;

public class ForceObjectiveMission : MonoBehaviour
{
    public MissionManager MissionManager;

    public void onbuttonexploded()
    {
        MissionManager.Instance.CompleteObjective();
    }
}
