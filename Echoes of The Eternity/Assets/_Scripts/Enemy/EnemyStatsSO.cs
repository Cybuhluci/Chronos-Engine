using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Scriptable Objects/EnemyStats")]
public class EnemyStatsSO : ScriptableObject
{
    [Header("Health")]
    public float MaxHealth = 100f;

    [Header("Movement")]
    public float WalkSpeed = 2.5f;
    public float RunSpeed = 4.5f;

    [Header("Combat")]
    public int ammoCount = 12; // how many times the enemy can attack before needing to reload
    public float DetectionRadius = 12f;
    public float AttackRange = 2f;
    public float AttackDamage = 10f;
    public float AttackRate = 1f; // attacks per second

    [Header("Misc")]
    public float KnockbackForce = 2f;
    public float DeathDelay = 5f;
}
