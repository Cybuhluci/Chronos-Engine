using UnityEngine;

public class ForceStartMission : MonoBehaviour
{
    public MissionManager MissionManager;

    public void onbuttonexploded()
    {
        MissionManager.Instance.StartMission(MissionManager.Instance.missionList.missions[0]);
    }
}
