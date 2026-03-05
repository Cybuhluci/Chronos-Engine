using Luci.Interactions;
using UnityEngine;

public class LootVanScript : MonoBehaviour
{
    public MissionManager _missionManager;
    public Collider lootVanCollider;

    private int storedBags = 0;
    private float storedEXP = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Loot"))
        {
            var loot = other.GetComponent<BagPickupScript>().lootSO;
            storedEXP += loot.lootEXP;
            _missionManager.AddBags(1, other.gameObject);
            storedBags++;
        }

        if (other.CompareTag("Player"))
        {
            if (_missionManager.IsHeistLeavable())
            {
                _missionManager.TryExitHeist();
            }
        }
    }

    public float GetStoredEXP()
    {
        return storedEXP;
    }

    public float GetStoredBags()
    {
        return storedBags;
    }
}
