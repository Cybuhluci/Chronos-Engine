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
    [SerializeField] private GameObject _miscBag;

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
        if (currentBags <= 0)
        {
            ConsoleManager.instance.AppendOutput("No bags to throw.");
            return;
        }

        GameObject currentBagObject;
        if (!_miscBag)
        {
            // Instantiate a bag prefab and apply force to it in the direction the player is facing
            currentBagObject = Instantiate(heldLoot.lootBagPrefab, weaponHolder.transform.position, weaponHolder.transform.rotation);
        }
        else
        {
            currentBagObject = Instantiate(_miscBag, weaponHolder.transform.position, weaponHolder.transform.rotation);
            _miscBag = null; // Clear the misc bag reference after throwing
        }

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

    public void AddMiscBag(GameObject miscBag)
    {
        _miscBag = miscBag;
        currentBags++;
        bag1UIPanel.SetActive(true);
        bag1UIText.text = miscBag.name;
        ConsoleManager.instance.AppendOutput($"Added {_miscBag.name} to the player's bag.");
    }

    public bool CanAddBag()
    {
        return currentBags < maxBags;
    }
}
