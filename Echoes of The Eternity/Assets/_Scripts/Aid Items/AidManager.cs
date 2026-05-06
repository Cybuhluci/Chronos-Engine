using UnityEngine;

public class AidManager : MonoBehaviour
{
    public PlayerAttributes playerAttributes;
    public PlayerHealth playerHealth;

    public void UseAidIem(AidSO aid)
    {
        AidEffectSO effect = aid.effect;
        // Implement the logic to use the aid item based on its type and properties.
        // This could involve healing the player, providing buffs, or other effects.
        Debug.Log($"Using aid item: {aid.aidName}");
    }
}
