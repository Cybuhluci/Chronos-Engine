using UnityEngine;

public class PlayerAttributes : MonoBehaviour
{
    public PlayerMovement PlayerMove;

    [Header("Player Type")]
    public PlayerType PlayType;
    public enum PlayerType { FirstPerson, ThirdPerson }

    public MovementType MoveType;
    public enum MovementType { Walk, Run, Sprint }

    void Awake()
    {
        if (PlayerMove == null)
            PlayerMove = GetComponent<PlayerMovement>();
    }
}
