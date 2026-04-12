using UnityEngine;

namespace Luci.Volumes
{

    public class SecureAreaVolume : MonoBehaviour
    {
        MissionManager missionManager;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            missionManager = MissionManager.Instance;
        }
    }
}