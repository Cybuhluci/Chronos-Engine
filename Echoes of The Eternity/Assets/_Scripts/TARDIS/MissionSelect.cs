using TARDIS.Main;
using UnityEngine;
using UnityEngine.UI;

public class MissionSelect : MonoBehaviour
{
    [SerializeField] Button mission1, mission2, mission3, mission4, mission5, mission6, mission7, neilsonsOffice, architectsCabin;

    public enum MissionLocation
    {
        Test,
        Heist1, // placeholder
        Heist3,
        Heist4,
        Heist5,
        Heist6,
        Heist7,
        Heist8,
        Heist9,
        Heist10,
        NeilsonsOffice,
        ArchitectsCabin
    }
    public MissionLocation selectedLocation;

    public void ChooseLocation(int missionIndex)
    {
        StoreMission(((MissionLocation)missionIndex).ToString());
    }

    string storedMission;
    public void StoreMission(string missionName)
    {
        storedMission = missionName;
        _42Main.Instance.SetFlightDestination(storedMission);
    }

    public string GetStoredMission()
    {
        return storedMission;
    }
}
