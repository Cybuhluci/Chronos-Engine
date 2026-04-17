using Luci;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunMainScript : MonoBehaviour
{
    public TMP_Text gunAmmo, gunAmmoType;

    [SerializeField] private bool isHolstered = false;
    bool lastEquippedGunWasEquipNowRatherThanBind = false;
    // used to determine whether to update the current slot when equipping a weapon directly (e.g. from pickup) vs equipping from the fixed inventory binds
    // mainly so the gun can be delted after unequipping it, since if it was an equipNow it should be deleted, but if it was from a bind slot it should stay in the slot but just be hidden

    [System.Serializable]
    public class WeaponInstance
    {
        public MainGunDataSO weaponData; // MainGunDataSO, GadgetDataSO, GrenadeDataSO
        public GameObject weaponModel;

        public string GetName()
        {
            if (weaponData == null) return "Empty";
            return weaponData.name;
        }
    }

    public InventoryManager inventoryManager;
    public FirstPersonController playerController;

    // Fixed 8-slot inventory
    public MainGunDataSO[] inventory;

    public PlayerInput playerInput;
    public GameObject weaponHolder; // assign in inspector


    [Header("Holster")]
    [Tooltip("Hold Reload this many seconds to toggle holster instead of reloading")]
    public float holsterHoldTime = 1f;
    private bool _reloadHeld = false;
    private float _reloadHoldTimer = 0f;

    [Header("Starting Loadout")]
    public MainGunDataSO[] StartingPrimary;

    // currentSlot is a 0-based index into the fixed 8-slot inventory.
    // A value of -1 means a non-bound (transient) weapon is currently equipped.
    public int currentSlot = 0;
    public GameObject currentlyEquippedWeapon;

    // runtime instances of bound weapon models (keeps instantiated GameObjects separate from the SO.model prefab reference)
    private GameObject[] _inventoryModelInstances;
    // instance for a non-bound weapon equipped via EquipWeaponNow
    private GameObject _transientEquippedInstance;

    private int ToIndex(int slotOneBased)
    {
        return Mathf.Clamp(slotOneBased - 1, 0, inventory.Length - 1);
    }

    [Header("Ammo")]
    [SerializeField] private TMP_Text primaryAmmo; // mag/reserve

    [SerializeField] private GameObject WoodPrefab, MetalPrefab, ConcretePrefab; // bullet hole prefabs for different materials

    /* 
    The Gun System: explained in words.

    the Gun Management System (tm) is responsible for managing weapon and grenade equipping, as well as instantiatiion.
    When the player uses their Paradigm Distortion Analyser (PDA) to select a weapon to equip, 
    it is the GMS's duty to equip to selected weapon or grenade into their respective slot.
    within the GMS, there is a fixed 8-slot cache inventory for bound weapons and aid items.
    this fixed-slot inventory is an already instantiated set of 8 weapons, which are instantiated when bound, and deleted when unbound.
    */

    // temp method for storing bound guns in a file
    public void SaveInventoryToFile()
    {
        string path = Application.persistentDataPath + "/Save/PlayerGunBinds.txt";
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < inventory.Length; i++)
        {
            sb.AppendLine($"Slot {i + 1}: {(inventory[i] != null ? inventory[i].name : "Empty")}");
        }
        System.IO.File.WriteAllText(path, sb.ToString());
        Debug.Log($"Inventory saved to {path}");
    }

    // method to load bound guns from file
    public void LoadInventoryFromFile()
    {
        string path = Application.persistentDataPath + "/Save/PlayerGunBinds.txt";
        if (!System.IO.File.Exists(path))
        {
            Debug.LogWarning($"Inventory file not found at {path}");
            return;
        }
        string[] lines = System.IO.File.ReadAllLines(path);
        for (int i = 0; i < lines.Length && i < inventory.Length; i++)
        {
            string line = lines[i];
            int slotIndex = i; // Assuming the file lines are in order of slots
            if (line.Contains("Empty"))
            {
                inventory[slotIndex] = null;
            }
            else
            {
                // Extract weapon name from line
                int colonIndex = line.IndexOf(':');
                if (colonIndex >= 0)
                {
                    string weaponName = line.Substring(colonIndex + 1).Trim();
                    // Here you would need a way to convert weaponName back to a MainGunDataSO reference.
                    // This could be done through a lookup dictionary or some other method depending on your project structure.
                    // For example:
                    // inventory[slotIndex] = InventoryManager.GetGunDataByName(weaponName);
                }
            }
        }
        Debug.Log($"Inventory loaded from {path}");
    }

    public void GiveControllersBulletHolePrefabs()
    {
        // include inactive so unloaded weapon models also receive the prefabs
        GunController[] controllers = GetComponentsInChildren<GunController>(true);
        foreach (var controller in controllers)
        {
            controller.SetBulletHolePrefabs(WoodPrefab, MetalPrefab, ConcretePrefab);
        }
    }

    // Called to initialize gun HUD/controllers when entering masked/combat mode
    public void StartGuns()
    {
        // update ammo UI on all GunController components that are part of this weapon holder
        if (weaponHolder != null)
        {
            var controllers = weaponHolder.GetComponentsInChildren<GunController>(true);
            foreach (var c in controllers)
            {
                if (c == null) continue;
                c.StartGun(this);
            }
        }
        else
        {
            var controllers = GetComponentsInChildren<GunController>(true);
            foreach (var c in controllers) c.UpdateAmmoUI();
        }
    }

    private void Start()
    {
        LoadInventoryFromFile();
        // initialize instance array to match inventory length
        _inventoryModelInstances = new GameObject[inventory.Length];
        AddWeaponToBindSlot(1, StartingPrimary[0], StartingPrimary[0].model);
        AddWeaponToBindSlot(2, StartingPrimary[1], StartingPrimary[1].model);
        AddWeaponToBindSlot(3, StartingPrimary[2], StartingPrimary[2].model);

        ShowOnlyCurrentWeaponModel();
        GiveControllersBulletHolePrefabs();

        StartGuns();

        SaveInventoryToFile();
    }

    private void Update()
    {
        if (playerInput != null)
        {
            if (playerController.CameraDisable) return; // don't allow any weapon input when camera is fully disabled (e.g. during PDA interaction)
            if (playerController.CameraHybridDisable && playerController.MovementDisable) return; 
            // don't allow weapon switching or firing when camera and movement is disabled (e.g. during dialogue or cutscene)

            // weapon slot switching
            if (playerInput.actions["1"].WasPressedThisFrame())
                SwitchToBindSlot(1);
            else if (playerInput.actions["2"].WasPressedThisFrame())
                SwitchToBindSlot(2);
            else if (playerInput.actions["3"].WasPressedThisFrame())
                SwitchToBindSlot(3);
            else if (playerInput.actions["4"].WasPressedThisFrame())
                SwitchToBindSlot(4);
            else if (playerInput.actions["5"].WasPressedThisFrame())
                SwitchToBindSlot(5);
            else if (playerInput.actions["6"].WasPressedThisFrame())
                SwitchToBindSlot(6);
            else if (playerInput.actions["7"].WasPressedThisFrame())
                SwitchToBindSlot(7);
            else if (playerInput.actions["8"].WasPressedThisFrame())
                SwitchToBindSlot(8);

            // Shooting
            if (playerInput != null && playerInput.actions["Fire"] != null && playerInput.actions["Fire"].IsPressed())
            {
                if (isHolstered || playerController.isSprinting) return; // don't allow firing when holstered or while sprinting.
                var gunController = GetCurrentWeaponModel()?.GetComponent<GunController>();
                if (gunController != null)
                {
                    bool fireInput = playerInput.actions["Fire"].IsPressed();
                    bool fireInputDown = playerInput.actions["Fire"].WasPressedThisFrame();
                    gunController.TryFire(fireInput, fireInputDown);
                }
            }

            // Reloading + holstering (same button)
            // pressing "Reload" will reload the weapon, holding will toggle the global holster state
            var reloadAction = playerInput.actions["Reload"];
            if (reloadAction != null)
            {
                if (reloadAction.WasPressedThisFrame())
                {
                    _reloadHeld = true;
                    _reloadHoldTimer = 0f;
                }

                if (_reloadHeld && reloadAction.IsPressed())
                {
                    _reloadHoldTimer += Time.deltaTime;
                    if (_reloadHoldTimer >= holsterHoldTime)
                    {
                        ToggleHolster();
                        // consume the hold so we don't immediately fire reload on release
                        _reloadHeld = false;
                        _reloadHoldTimer = 0f;
                    }
                }

                if (reloadAction.WasReleasedThisFrame())
                {
                    // short press -> reload
                    if (_reloadHoldTimer < holsterHoldTime)
                    {
                        var gunController = GetCurrentWeaponModel()?.GetComponent<GunController>();
                        gunController?.Reload();
                    }
                    _reloadHeld = false;
                    _reloadHoldTimer = 0f;
                }
            }
        }
    }

    private void ShowOnlyCurrentWeaponModel()
    {
        // If holstered, hide everything
        if (isHolstered)
        {
            if (_inventoryModelInstances != null)
            {
                for (int i = 0; i < _inventoryModelInstances.Length; i++)
                {
                    if (_inventoryModelInstances[i] != null)
                        _inventoryModelInstances[i].SetActive(false);
                }
            }

            if (_transientEquippedInstance != null)
                _transientEquippedInstance.SetActive(false);

            currentlyEquippedWeapon = null;
            return;
        }

        // Hide all bound instances, show only current
        for (int i = 0; i < inventory.Length; i++)
        {
            if (_inventoryModelInstances != null && _inventoryModelInstances.Length > i && _inventoryModelInstances[i] != null)
            {
                _inventoryModelInstances[i].SetActive(i == currentSlot);
            }
        }

        // Handle transient equipped instance
        if (_transientEquippedInstance != null)
        {
            _transientEquippedInstance.SetActive(currentSlot == -1);
        }

        // update currentlyEquippedWeapon reference
        if (currentSlot == -1)
        {
            currentlyEquippedWeapon = _transientEquippedInstance;
        }
        else if (currentSlot >= 0 && _inventoryModelInstances != null && currentSlot < _inventoryModelInstances.Length)
        {
            currentlyEquippedWeapon = _inventoryModelInstances[currentSlot];
        }
        else
        {
            currentlyEquippedWeapon = null;
        }
    }

    public void ToggleHolster()
    {
        isHolstered = !isHolstered;
        ShowOnlyCurrentWeaponModel();
    }

    public void SwitchToBindSlot(int slot)
    {
        // different version of equipfrombind which instead just setactive-falses the current weapon and setactive-trues the new bind slot weapon.
        // however, if the current weapon was equipped via equipnow (transient), then it will be deleted and the new weapon will be shown from the inventory binds as usual.

        // if the old weapon was a transient equip, delete it since it's not part of the inventory binds and won't be needed anymore
        if (_transientEquippedInstance != null)
        {
            Destroy(_transientEquippedInstance);
            _transientEquippedInstance = null;
        }

        // then switch to the chosen slot, by making the slot-to-be active. if the slot is empty, do nothing.

        int idx = ToIndex(slot);
        if (inventory == null || idx < 0 || idx >= inventory.Length) return;
        if (inventory[idx] == null)
        {
            Debug.LogWarning($"Attempted to equip empty slot {slot}");
            return;
        }

        currentSlot = idx;
        ShowOnlyCurrentWeaponModel();

        //// Initialize controllers for this weapon if present
        //var controller = GetCurrentWeaponModel()?.GetComponent<GunController>();
        //controller?.StartGun();
    }

    public void EquipWeaponNow(MainGunDataSO data, GameObject model)
    {
        // check to see if it's already in the inventory binds, if it is then just switch to that slot,
        // if not then instantiate it directly in the weapon holder as a transient instance that isn't part of the inventory binds (and will be deleted on unequip)



        // Instantly equips the weapon in the player's hands (non-bound/equipped directly).
        if (data == null) return;
        // If this weapon exists in a bind slot, switch to that slot and show its instance.
        int found = -1;
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] == data)
            {
                found = i;
                break;
            }
        }

        // clean up any previous transient instance
        if (_transientEquippedInstance != null)
        {
            Destroy(_transientEquippedInstance);
            _transientEquippedInstance = null;
        }

        if (found >= 0)
        {
            currentSlot = found;
        }
        else
        {
            // instantiate a transient instance of the weapon model (prefer provided model, else use SO.model prefab)
            GameObject instance = null;
            if (model != null)
            {
                instance = Instantiate(model, weaponHolder != null ? weaponHolder.transform : null);
            }
            else if (data.model != null)
            {
                instance = Instantiate(data.model, weaponHolder != null ? weaponHolder.transform : null);
            }

            if (instance != null)
            {
                instance.SetActive(true);
            }

            _transientEquippedInstance = instance;
            // indicate it's a transient equip so it can be deleted on unequip if necessary
            lastEquippedGunWasEquipNowRatherThanBind = true;
            currentSlot = -1;

            GunController controller2 = instance.GetComponent<GunController>();
            controller2?.StartGun(this);
            controller2?.SetBulletHolePrefabs(WoodPrefab, MetalPrefab, ConcretePrefab);
            controller2?.UpdateAmmoUI();
        }

        ShowOnlyCurrentWeaponModel();
    }

    public void AddWeaponToBindSlot(int slot, MainGunDataSO data, GameObject model)
    {
        int idx = ToIndex(slot);
        if (data == null) return;

        // assign the data to the bind slot
        inventory[idx] = data;

        // destroy previous instance if exists
        if (_inventoryModelInstances != null && _inventoryModelInstances.Length > idx && _inventoryModelInstances[idx] != null)
        {
            Destroy(_inventoryModelInstances[idx]);
            _inventoryModelInstances[idx] = null;
        }

        // Instantiate a runtime instance for this bound weapon. Prefer a provided model instance/prefab, else use the SO.model prefab.
        GameObject instance = null;
        if (model != null)
        {
            instance = Instantiate(model, weaponHolder != null ? weaponHolder.transform : null);
        }
        else if (data.model != null)
        {
            instance = Instantiate(data.model, weaponHolder != null ? weaponHolder.transform : null);
        }

        if (instance != null)
        {
            instance.SetActive(false);
            if (_inventoryModelInstances == null || _inventoryModelInstances.Length != inventory.Length)
            {
                _inventoryModelInstances = new GameObject[inventory.Length];
            }
            _inventoryModelInstances[idx] = instance;
        }
    }

    public void RemoveWeaponFromBindSlot(int slot)
    {
        int idx = ToIndex(slot);
        if (idx < 0 || idx >= inventory.Length) return;

        var data = inventory[idx];
        // destroy any instantiated model for this slot
        if (_inventoryModelInstances != null && idx < _inventoryModelInstances.Length && _inventoryModelInstances[idx] != null)
        {
            Destroy(_inventoryModelInstances[idx]);
            _inventoryModelInstances[idx] = null;
        }

        inventory[idx] = null;
    }

    // Get the current weapon's model GameObject
    public GameObject GetCurrentWeaponModel() // this works, but not well, instead it will just be held in "currentlyequippedweapon"
    {
        if (inventory == null) return null;
        if (currentSlot == -1)
        {
            return _transientEquippedInstance;
        }

        if (currentSlot < 0 || currentSlot >= inventory.Length) return null;
        var data = inventory[currentSlot];
        if (data == null) return null;
        if (_inventoryModelInstances != null && currentSlot < _inventoryModelInstances.Length && _inventoryModelInstances[currentSlot] != null)
            return _inventoryModelInstances[currentSlot];

        // fallback to SO model reference (not instantiated)
        return data.model;
    }

    public GunController GunController()
    {
        return GetCurrentWeaponModel()?.GetComponent<GunController>();
    }
}
