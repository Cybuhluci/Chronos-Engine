using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunMainScript : MonoBehaviour
{
    public enum WeaponSlot
    {
        Primary = 0,
        Secondary = 1,
        Gadget1 = 2,
        Gadget2 = 3,
        Grenade = 4
    }

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
    public MainGunDataSO StartingPrimary;
    public MainGunDataSO StartingSecondary;
    public GadgetDataSO StartingGadget1, StartingGadget2;
    public GrenadeDataSO StartingGrenade;
    public GameObject isPrimaryActive;

    public WeaponSlot currentSlot = WeaponSlot.Primary;

    public TMP_Text weaponNameText; // UI Text to display weapon name

    private float weaponNameVisibleTime = 0.5f;
    private float weaponNameTimer = 0f;
    private bool weaponNameVisible = true;
    [SerializeField] private TMP_Text primaryAmmo;
    [SerializeField] private TMP_Text primaryAmmoReserve;
    [SerializeField] private TMP_Text secondaryAmmo;

    [SerializeField] private GameObject WoodPrefab, MetalPrefab, ConcretePrefab; // bullet hole prefabs for different materials

    public void GiveControllersBulletHolePrefabs()
    {
        // include inactive so unloaded weapon models also receive the prefabs
        GunController[] controllers = GetComponentsInChildren<GunController>(true);
        foreach (var controller in controllers)
        {
            controller.SetBulletHolePrefabs(WoodPrefab, MetalPrefab, ConcretePrefab);
        }
    }

    public void ChangeWeapon()
    {
        secondaryAmmo.text = primaryAmmo.text + primaryAmmoReserve.text;
    }

    private void Start()
    {
        for (int i = 0; i < inventory.Length; i++) inventory[i] = new WeaponInstance();

        if (StartingPrimary != null) EquipWeapon(WeaponSlot.Primary, StartingPrimary, StartingPrimary.model);
        if (StartingSecondary != null) EquipWeapon(WeaponSlot.Secondary, StartingSecondary, StartingSecondary.model);
        if (StartingGadget1 != null) EquipWeapon(WeaponSlot.Gadget1, StartingGadget1, StartingGadget1.model);
        if (StartingGadget2 != null) EquipWeapon(WeaponSlot.Gadget2, StartingGadget2, StartingGadget2.model);
        if (StartingGrenade != null) EquipWeapon(WeaponSlot.Grenade, StartingGrenade, StartingGrenade.model);

        UpdateWeaponNameText();
        ShowOnlyCurrentWeaponModel();

        secondaryAmmo.text = StartingSecondary.magazineSize.ToString() + "/" + StartingSecondary.reserveAmmo.ToString();
        GiveControllersBulletHolePrefabs();
    }

    private void Update()
    {
        UpdateWeaponNameText();

        if (playerInput != null)
        {
            if (playerInput.actions["Primary"] != null && playerInput.actions["Primary"].WasPressedThisFrame())
                SwitchToSlot(WeaponSlot.Primary);
            else if (playerInput.actions["Secondary"] != null && playerInput.actions["Secondary"].WasPressedThisFrame())
                SwitchToSlot(WeaponSlot.Secondary);
            else if (playerInput.actions["Secondary"] != null && playerInput.actions["Secondary"].WasPressedThisFrame())
                SwitchToSlot(WeaponSlot.Secondary);
            else if (playerInput.actions["Secondary"] != null && playerInput.actions["Secondary"].WasPressedThisFrame())
                SwitchToSlot(WeaponSlot.Secondary);
            else if (playerInput.actions["Gadget1"] != null && playerInput.actions["Gadget1"].WasPressedThisFrame())
                SwitchToSlot(WeaponSlot.Gadget1);
            else if (playerInput.actions["Gadget2"] != null && playerInput.actions["Gadget2"].WasPressedThisFrame())
                SwitchToSlot(WeaponSlot.Gadget2);
            else if (playerInput.actions["Grenade"] != null && playerInput.actions["Grenade"].WasPressedThisFrame())
                SwitchToSlot(WeaponSlot.Grenade);
        }

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

        // Fade out weapon name after a delay
        if (weaponNameVisible)
        {
            weaponNameTimer += Time.deltaTime;
            if (weaponNameTimer > weaponNameVisibleTime)
            {
                SetWeaponNameAlpha(0f);
                weaponNameVisible = false;
            }
        }
    }

    public void SwitchToSlot(WeaponSlot slot)
    {
        if (currentSlot == slot) return; // already in this slot

        ChangeWeapon();
        currentSlot = slot;
        ShowOnlyCurrentWeaponModel();
        weaponNameTimer = 0f;
        SetWeaponNameAlpha(1f);
        weaponNameVisible = true;
        UpdateWeaponNameText();

        // if primary weapon is active, make "isPrimaryActive" true
        if (isPrimaryActive != null)
            isPrimaryActive.SetActive(currentSlot == WeaponSlot.Primary);
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
    public void EquipWeapon(WeaponSlot slot, ScriptableObject data, GameObject modelPrefab)
    {
        int index = (int)slot;
        if (inventory[index] == null) inventory[index] = new WeaponInstance();

        if (inventory[index].weaponModel != null)
            Destroy(inventory[index].weaponModel);

        inventory[index].weaponData = data;
        if (modelPrefab != null)
        {
            GameObject model = Instantiate(modelPrefab, weaponHolder.transform);
            model.SetActive(false);
            inventory[index].weaponModel = model;
            // give new model's controllers the bullet hole prefabs immediately
            GiveControllersBulletHolePrefabs();
        }
        else
        {
            inventory[index].weaponModel = null;
        }

        if (currentSlot == slot) ShowOnlyCurrentWeaponModel();
        UpdateWeaponNameText();
    }

    // Convenience: add a MainGun into the first available slot (primary then secondary then gadgets)
    public void AddNewWeapon(MainGunDataSO weaponData)
    {
        if (weaponData == null) return;
        WeaponSlot[] preferred = new[] { WeaponSlot.Primary, WeaponSlot.Secondary, WeaponSlot.Gadget1, WeaponSlot.Gadget2, WeaponSlot.Grenade };
        foreach (var s in preferred)
        {
            if (inventory[(int)s].weaponData == null)
            {
                EquipWeapon(s, weaponData, weaponData.model);
                currentSlot = s;
                ShowOnlyCurrentWeaponModel();
                UpdateWeaponNameText();
                return;
            }
        }
        // If all full, replace current slot
        EquipWeapon(currentSlot, weaponData, weaponData.model);
    }

    public void RemoveWeapon(WeaponSlot slot)
    {
        int index = (int)slot;
        if (inventory[index] == null) return;
        if (inventory[index].weaponModel != null) Destroy(inventory[index].weaponModel);
        inventory[index].weaponModel = null;
        inventory[index].weaponData = null;
        if (currentSlot == slot) UpdateWeaponNameText();
    }

    private void UpdateWeaponNameText()
    {
        if (weaponNameText == null) return;
        var inst = inventory[(int)currentSlot];
        if (inst != null && inst.weaponData != null)
        {
            weaponNameText.text = inst.GetName();
            weaponNameTimer = 0f;
            SetWeaponNameAlpha(1f);
            weaponNameVisible = true;
        }
        else
        {
            weaponNameText.text = "";
        }
    }

    void SetWeaponNameAlpha(float alpha)
    {
        if (weaponNameText == null) return;
        var c = weaponNameText.color;
        c.a = alpha;
        weaponNameText.color = c;
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
