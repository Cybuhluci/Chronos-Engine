using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    public enum Difficulty
    {
        Easy,
        Normal,
        Hard,
        VeryHard,
        Overkill,
        Mayhem,
    }
    public Difficulty currentDifficulty = Difficulty.Normal;

    [SerializeField] private PlayerInput _PlayerInput;
    [SerializeField] private GunMainScript _GunMainScript;

    private void Update()
    {

    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }
}