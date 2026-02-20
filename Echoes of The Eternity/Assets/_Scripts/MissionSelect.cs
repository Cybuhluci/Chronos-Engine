using UnityEngine;
using UnityEngine.UI;

public class MissionSelect : MonoBehaviour
{
    [SerializeField] Button mission1, mission2, mission3, mission4, mission5, mission6, mission7, neilsonsOffice, architectsCabin;

    public enum MissionLocation
    {
        Mission1,
        Mission2,
        Mission3,
        Mission4,
        Mission5,
        Mission6,
        Mission7,
        NeilsonsOffice,
        ArchitectsCabin
    }
    public MissionLocation selectedLocation;

    public void ChooseLocation(int missionIndex)
    {
        selectedLocation = (MissionLocation)missionIndex;
        Debug.Log("Location Chosen: " + selectedLocation);
    }
}
