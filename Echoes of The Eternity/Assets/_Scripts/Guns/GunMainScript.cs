using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GunMainScript : MonoBehaviour
{
    bool isHolstering = false;

    [System.Serializable]
    public class WeaponInstance
    {
        public ScriptableObject weaponData; // MainGunDataSO, GadgetDataSO, GrenadeDataSO
        public GameObject weaponModel;

        public string GetName()
        {
            if (weaponData == null) return "Empty";
            if (weaponData is MainGunDataSO g) return g.weaponName;
            if (weaponData is GadgetDataSO gd) return gd.weaponName;
            if (weaponData is GrenadeDataSO gr) return gr.weaponName;
            return weaponData.name;
        }
    }

    // Fixed 5-slot inventory
    public WeaponInstance[] inventory = new WeaponInstance[5];

    public PlayerInput playerInput;
    public GameObject weaponHolder; // assign in inspector

    [Header("Starting Loadout")]
    public MainGunDataSO[] StartingPrimary;

    public int currentSlot = 1;

    [Header("Ammo")]
    [SerializeField] private TMP_Text primaryAmmo; // mag/reserve

    [Header("Unique Deployable")]
    [SerializeField] private GameObject UDInsantObject;

    [SerializeField] private GameObject WoodPrefab, MetalPrefab, ConcretePrefab; // bullet hole prefabs for different materials

    bool maskedUp = false;

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
                c.StartGun();
            }
        }
        else
        {
            var controllers = GetComponentsInChildren<GunController>(true);
            foreach (var c in controllers) c.UpdateAmmoUI();
        }
    }

    // Called to initialize gadget HUD/controllers when entering masked/combat mode
    public void StartGadgets()
    {
        // intentionally named per project convention (StartGafgets)
        var gadgets = weaponHolder.GetComponentsInChildren<GadgetController>(true); ;
        foreach (var g in gadgets)
        {
            if (g == null) continue;
            g.StartGadget();
        }
    }

    // Called to initialize grenade HUD/controllers when entering masked/combat mode
    public void StartGrenade()
    {
        var grenades = weaponHolder.GetComponentsInChildren<GrenadeController>(true);
        foreach (var gr in grenades)
        {
            if (gr == null) continue;
            gr.StartGrenade();
        }
    }

    private void Start()
    {
        for (int i = 0; i < inventory.Length; i++) inventory[i] = new WeaponInstance();

        if (StartingPrimary != null && StartingPrimary.Length > 0)
        {
            EquipWeapon(1, StartingPrimary[0], StartingPrimary[0].model);
        }

        ShowOnlyCurrentWeaponModel();
        GiveControllersBulletHolePrefabs();


        // equip all the guns, but do not go into masked-up mode.
        // then unequiptocasing so the player has all the HUD assets actually showing,
        // then the player can maskup and equip the primary weapon to then begin shooting shit.


        // initialize HUD and controllers for guns, gadgets and grenade
        StartGuns();
    }

    private void Update()
    {
        if (playerInput != null)
        {
            // weapon slot switching
            if (playerInput.actions["1"].WasPressedThisFrame())
                SwitchToSlot(1);
            else if (playerInput.actions["2"].WasPressedThisFrame())
                SwitchToSlot(2);
            else if (playerInput.actions["3"].WasPressedThisFrame())
                SwitchToSlot(3);
            else if (playerInput.actions["4"].WasPressedThisFrame())
                SwitchToSlot(4);
            else if (playerInput.actions["5"].WasPressedThisFrame())
                SwitchToSlot(5);
            else if (playerInput.actions["6"].WasPressedThisFrame())
                SwitchToSlot(6);
            else if (playerInput.actions["7"].WasPressedThisFrame())
                SwitchToSlot(7);
            else if (playerInput.actions["8"].WasPressedThisFrame())
                SwitchToSlot(8);

            // Shooting
            if (playerInput != null && playerInput.actions["Fire"] != null && playerInput.actions["Fire"].IsPressed())
            {
                var gunController = GetCurrentWeaponModel()?.GetComponent<GunController>();
                if (gunController != null)
                {
                    bool fireInput = playerInput.actions["Fire"].IsPressed();
                    bool fireInputDown = playerInput.actions["Fire"].WasPressedThisFrame();
                    gunController.TryFire(fireInput, fireInputDown);
                }
            }

            // Reloading
            if (playerInput != null && playerInput.actions["Reload"] != null && playerInput.actions["Reload"].WasPressedThisFrame())
            {
                var gunController = GetCurrentWeaponModel()?.GetComponent<GunController>();
                gunController?.Reload();
            }
        }
    }

    public void SwitchToSlot(int slot)
    {
        if (currentSlot == slot) /* enequip gun */ return;

        currentSlot = slot;
        ShowOnlyCurrentWeaponModel();
    }

    private void ShowOnlyCurrentWeaponModel()
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] != null && inventory[i].weaponModel != null)
                inventory[i].weaponModel.SetActive(i == (int)currentSlot);
        }
    }

    // Equip any ScriptableObject-based weapon into a slot
    public void EquipWeapon(int slot, ScriptableObject data, GameObject modelPrefab)
    {
        // puts weapon in the 8-weapon list that is instantiated for quicker access.
    }

    public void AddNewWeapon(MainGunDataSO weaponData)
    {
        // add gun to a big list of owned guns - add to the first available slot and add to inventory, if no slots just shove it in the inventory.
    }

    public void RemoveWeapon(int slot)
    {
        int index = slot;
        if (inventory[index] == null) return;
        if (inventory[index].weaponModel != null) Destroy(inventory[index].weaponModel);
        inventory[index].weaponModel = null;
        inventory[index].weaponData = null;
    }

    // Get the current weapon's ScriptableObject
    public ScriptableObject GetCurrentWeaponData()
    {
        return inventory[(int)currentSlot].weaponData;
    }

    // Get the current weapon's model GameObject
    public GameObject GetCurrentWeaponModel()
    {
        return inventory[(int)currentSlot].weaponModel;
    }

    public GunController GunController()
    {
        return GetCurrentWeaponModel()?.GetComponent<GunController>();
    }
}
