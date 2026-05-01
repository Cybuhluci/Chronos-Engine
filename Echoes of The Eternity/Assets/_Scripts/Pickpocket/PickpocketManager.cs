using Luci;
using Luci.Interactions;
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

        ResetItems();

        // Populate player inventory UI
        foreach (InventoryItemSO item in playerInventoryManager.GetInventoryItems())
        {
            var itemUI = Instantiate(inventoryItemPrefab, playerInventoryParent);
            itemUI.GetComponentInChildren<TextMeshProUGUI>().text = item.itemName;
            // Add functionality to item UI, find button and assign transfer method
            var transferToCharacterButton = itemUI.transform.GetComponent<Button>();
            transferToCharacterButton.onClick.AddListener(() => TransferItemToCharacter(item));
            // make sure that if an item already exists then a new button isnt made,
            // and instead the existing button gets a "(xn)" added to the end of the item name to show how many of that item there are,
            // and the transfer method is updated to use a new multi-item transfer method that allows the player to choose how many of that item they want to transfer
        }

        // Populate character inventory UI
        foreach (InventoryItemSO item in currentNPC.inventoryItemSOs)
        {
            var itemUI = Instantiate(inventoryItemPrefab, characterInventoryParent);
            itemUI.GetComponentInChildren<TextMeshProUGUI>().text = item.itemName;
            // Add functionality to item UI, find button and assign transfer method
            var transferToPlayerButton = itemUI.transform.GetComponent<Button>();
            transferToPlayerButton.onClick.AddListener(() => TransferItemToPlayer(item));
            // make sure that if an item already exists then a new button isnt made,
            // and instead the existing button gets a "(xn)" added to the end of the item name to show how many of that item there are,
            // and the transfer method is updated to use a new multi-item transfer method that allows the player to choose how many of that item they want to transfer
        }
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

        // Repopulate player inventory UI
        foreach (var item in playerInventoryManager.GetInventoryItems())
        {
            var itemUI = Instantiate(inventoryItemPrefab, playerInventoryParent);
            itemUI.GetComponentInChildren<TextMeshProUGUI>().text = item.itemName;
            // Add functionality to item UI, find button and assign transfer method
            var transferToCharacterButton = itemUI.transform.GetComponent<Button>();
            transferToCharacterButton.onClick.AddListener(() => TransferItemToCharacter(item));
            // make sure that if an item already exists then a new button isnt made,
            // and instead the existing button gets a "(xn)" added to the end of the item name to show how many of that item there are,
            // and the transfer method is updated to use a new multi-item transfer method that allows the player to choose how many of that item they want to transfer
        }

        // Populate character inventory UI
        foreach (var item in currentNPC.inventoryItemSOs)
        {
            var itemUI = Instantiate(inventoryItemPrefab, characterInventoryParent);
            itemUI.GetComponentInChildren<TextMeshProUGUI>().text = item.itemName;
            // Add functionality to item UI, find button and assign transfer method
            var transferToPlayerButton = itemUI.transform.GetComponent<Button>();
            transferToPlayerButton.onClick.AddListener(() => TransferItemToPlayer(item));
            // make sure that if an item already exists then a new button isnt made,
            // and instead the existing button gets a "(xn)" added to the end of the item name to show how many of that item there are,
            // and the transfer method is updated to use a new multi-item transfer method that allows the player to choose how many of that item they want to transfer
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
