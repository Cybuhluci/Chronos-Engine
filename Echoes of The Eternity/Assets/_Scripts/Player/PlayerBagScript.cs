using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBagScript : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput; // Reference to the PlayerInput component for handling input
    public int maxBags = 1;
    [SerializeField] private int currentBags = 0;
    public LootSO heldLoot;
    [SerializeField] private Transform weaponHolder; // Reference to the transform where the bag will be thrown from

    [Header("Bag UI Elements")]
    [SerializeField] private GameObject bag1UIPanel;
    [SerializeField] private TMP_Text bag1UIText;

    // Update is called once per frame
    void Update()
    {
        if (playerInput.actions["ThrowBag"].WasPressedThisFrame())
        {
            ThrowBag();
        }
    }

    public void ThrowBag()
    {
        // Instantiate a bag prefab and apply force to it in the direction the player is facing
        GameObject currentBagObject = Instantiate(heldLoot.lootBagPrefab, weaponHolder.transform.position, weaponHolder.transform.rotation);
        Rigidbody rb = currentBagObject.GetComponentInChildren<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Camera.main.transform.forward * 10f; // throw forward with some speed
        }

        heldLoot = null;
        currentBags--;
        bag1UIText.text = "";
        bag1UIPanel.SetActive(false);
        ConsoleManager.instance.AppendOutput("Threw the bag.");
    }

    public void AddLoot(LootSO lootSO)
    {
        heldLoot = lootSO;
        currentBags++;
        bag1UIPanel.SetActive(true);
        bag1UIText.text = lootSO.name;

        ConsoleManager.instance.AppendOutput($"Added {lootSO.lootName} to the player's bag.");
    }

    public bool CanAddBag()
    {
        return currentBags < maxBags;
    }
}
