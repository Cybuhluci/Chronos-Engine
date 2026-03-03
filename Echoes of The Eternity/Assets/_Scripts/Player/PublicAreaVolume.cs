using UnityEngine;

namespace Luci.Volumes
{
    public class PublicAreaVolume : MonoBehaviour
    {
        MissionManager missionManager;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            missionManager = MissionManager.Instance;
        }

        private void OnTriggerStay(Collider other)
        {
            missionManager.currentPlayerLocation = MissionManager.PlayerLocation.Public;
        }
    }

}