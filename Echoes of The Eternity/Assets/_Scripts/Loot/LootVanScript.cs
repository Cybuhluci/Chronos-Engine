using UnityEngine;

public class LootVanScript : MonoBehaviour
{
    public MissionManager _missionManager;
    public Collider lootVanCollider;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Loot"))
        {
            _missionManager.AddBags(1, other.gameObject);
        }

        if (other.CompareTag("Player"))
        {
            if (_missionManager.IsHeistLeavable())
            {
                _missionManager.TryExitHeist();
            }
        }
    }
}
