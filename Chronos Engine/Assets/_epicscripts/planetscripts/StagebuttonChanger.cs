using UnityEngine;

public class StagebuttonChanger : MonoBehaviour
{
    [SerializeField] private string GoToLocation;
    [SerializeField] private StageManager StageManager;

    void Start()
    {
        StageManager = FindAnyObjectByType<StageManager>();
    }

    void Awake()
    {
        StageManager = FindAnyObjectByType<StageManager>();
    }

    public void StageButtonuse()
    {
        StageManager.LoadMiscScene(GoToLocation);
    }
}
