using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;

    [Header("Mission UI Elements")]
    public TMP_Text currentMissionOutcomeText;
    public TMP_Text currentMissionNameText;
    public TMP_Text nextMissionText; // Just says "NEXT MISSION"
    public TMP_Text nextMissionNameText; // Displays next mission's name

    private int currentMissionIndex = 0;
    public Mission activeMission = null;
    public missionLIST missionList;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Set default UI text
        nextMissionText.text = "NEXT MISSION";

        missionList.PopulateMissions();

        StartMission(missionList.missions[0]);
    }

    public void StartMission(Mission mission)
    {
        if (activeMission != null && !activeMission.isCompleted)
        {
            Debug.LogWarning("Cannot start a new mission until the current one is completed!");
            return;
        }

        activeMission = mission;
        activeMission.isActive = true;

        // Update UI
        UpdateMissionUI();

        Debug.Log("Started mission: " + mission.missionName);
    }

    public void CompleteObjective()
    {
        if (activeMission == null) return;

        activeMission.CompleteObjective();
        Debug.Log("Objective Complete: " + activeMission.GetCurrentObjective());

        if (activeMission.isCompleted)
        {
            Debug.Log("Mission Completed: " + activeMission.missionName);
            MoveToNextMission();
        }

        // Update UI
        UpdateMissionUI();
    }

    public void FailMission()
    {
        if (activeMission == null) return;

        activeMission.FailMission();
        Debug.Log("Mission Failed: " + activeMission.missionName);

        // Update UI
        UpdateMissionUI();
    }

    public void MoveToNextMission()
    {
        if (currentMissionIndex + 1 < missionList.missions.Count)
        {
            currentMissionIndex++;
            StartMission(missionList.missions[currentMissionIndex]);
        }
        else
        {
            activeMission = null;
            Debug.Log("All missions completed!");
        }

        UpdateMissionUI();
    }

    private void UpdateMissionUI()
    {
        if (activeMission != null)
        {
            currentMissionNameText.text = activeMission.missionName;

            if (activeMission.isFailed)
                currentMissionOutcomeText.text = "<color=red>MISSION FAILED!</color>";
            else if (activeMission.isCompleted)
                currentMissionOutcomeText.text = "<color=green>MISSION COMPLETED!</color>";
            else
                currentMissionOutcomeText.text = "Objective: " + activeMission.GetCurrentObjective();
        }
        else
        {
            currentMissionNameText.text = "";
            currentMissionOutcomeText.text = "No active mission";
        }

        // Display the next mission if available
        if (currentMissionIndex + 1 < missionList.missions.Count)
        {
            nextMissionNameText.text = missionList.missions[currentMissionIndex + 1].missionName;
        }
        else
        {
            nextMissionNameText.text = "No further missions";
        }
    }
}

[System.Serializable]
public class Mission
{
    public string missionName;
    public string description;
    public bool isActive;
    public bool isCompleted;
    public bool isFailed;
    public List<string> objectives;
    public int currentObjectiveIndex;

    public Mission(string name, string desc, List<string> objs)
    {
        missionName = name;
        description = desc;
        objectives = objs;
        currentObjectiveIndex = 0;
        isActive = false;
        isCompleted = false;
        isFailed = false;
    }

    public string GetCurrentObjective()
    {
        if (currentObjectiveIndex < objectives.Count)
            return objectives[currentObjectiveIndex];
        return "Mission Completed!";
    }

    public void CompleteObjective()
    {
        if (currentObjectiveIndex < objectives.Count - 1)
            currentObjectiveIndex++;
        else
            isCompleted = true;
    }

    public void FailMission()
    {
        isFailed = true;
    }
}
