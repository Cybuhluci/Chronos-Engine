using Luci;
using Luci.Interactions;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PickpocketManager : MonoBehaviour
{
    public static PickpocketManager Instance { get; private set; }
    public PlayerInput playerInput;
    public GameObject pickpocketUI;
    public Transform playerInventoryParent;
    public Transform characterInventoryParent;
    public FirstPersonController playerController;
    public InventoryManager playerInventoryManager;
    public GameObject inventoryItemPrefab;
    public GameObject playerHUD;
    public PcNpcInteractScript currentNPC; // Reference to the NPC interaction script to access character data

    private CharacterData characterData; // Store the current character data being pickpocketed

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public void AttemptPickpocket(CharacterData characterData, PcNpcInteractScript npc)
    {
        this.characterData = characterData; // Store the current character data being pickpocketed
        currentNPC = npc;
        pickpocketUI.SetActive(true);

        playerController.ToggleDisableCamera(true);
        playerController.ToggleDisableMovement(true);
        Cursor.lockState = CursorLockMode.Confined;
        playerHUD.SetActive(false);

        UpdateUI();
    }

    public void TransferItemToPlayer(InventoryItemSO item)
    {
        playerInventoryManager.AddItem(item);
        // Remove item from character's inventory (not shown here)
        currentNPC.RemoveInventoryItem(item);
        // Update UI accordingly
        UpdateUI();
    }

    public void TransferItemToCharacter(InventoryItemSO item)
    {
        playerInventoryManager.RemoveItem(item);
        // Add item to character's inventory (not shown here)
        currentNPC.AddInventoryItem(item);
        // Update UI accordingly
        UpdateUI();
    }

    public void ClosePickpocketUI()
    {
        ResetItems();
        pickpocketUI.SetActive(false);

        playerController.ToggleDisableCamera(false);
        playerController.ToggleDisableMovement(false);
        Cursor.lockState = CursorLockMode.Locked;
        playerHUD.SetActive(true);
    }

    private void UpdateUI()
    {
        ResetItems();

        // Repopulate player inventory UI with grouped items
        var groupedPlayerItems = playerInventoryManager.GetInventoryItems().GroupBy(i => i);
        foreach (var group in groupedPlayerItems)
        {
            var item = group.Key;
            int count = group.Count();

            var itemUI = Instantiate(inventoryItemPrefab, playerInventoryParent);
            itemUI.GetComponentInChildren<TextMeshProUGUI>().text = count > 1 ? $"{item.itemName} (x{count})" : item.itemName;
            
            var transferToCharacterButton = itemUI.transform.GetComponent<Button>();
            transferToCharacterButton.onClick.AddListener(() => TransferItemToCharacter(item));
        }

        // Populate character inventory UI with grouped items
        var groupedNPCItems = currentNPC.inventoryItemSOs.GroupBy(i => i);
        foreach (var group in groupedNPCItems)
        {
            var item = group.Key;
            int count = group.Count();

            var itemUI = Instantiate(inventoryItemPrefab, characterInventoryParent);
            itemUI.GetComponentInChildren<TextMeshProUGUI>().text = count > 1 ? $"{item.itemName} (x{count})" : item.itemName;
            
            var transferToPlayerButton = itemUI.transform.GetComponent<Button>();
            transferToPlayerButton.onClick.AddListener(() => TransferItemToPlayer(item));
        }
    }

    private void ResetItems()
    {
        foreach (Transform child in playerInventoryParent)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in characterInventoryParent)
        {
            Destroy(child.gameObject);
        }
    }
}
