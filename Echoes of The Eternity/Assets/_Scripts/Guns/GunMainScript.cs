using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    public SignatureDeployableSO StartingSignature;
    public UniqueDeployableSO StartingUniqueDeployable;
    public GameObject isPrimaryActive;
    private GameObject SignatureDeployable;
    private float signatureCooldownRemaining = 0f;
    // signature / unique hold state
    private bool _sigHoldInProgress = false;
    private float _sigHoldTimer = 0f;
    private bool _sigHoldTriggered = false; // whether unique action triggered from hold

    public WeaponSlot currentSlot = WeaponSlot.Primary;

    public TMP_Text weaponNameText; // UI Text to display weapon name

    private float weaponNameVisibleTime = 0.5f;
    private float weaponNameTimer = 0f;
    private bool weaponNameVisible = true;

    [Header("Ammo")]
    [SerializeField] private TMP_Text primaryAmmo;
    [SerializeField] private TMP_Text primaryAmmoReserve;
    [SerializeField] private TMP_Text secondaryAmmo;
    [SerializeField] private Image SignatureCooldown;

    [Header("Unique Deployable")]
    [SerializeField] private GameObject UDInsantObject;
    private bool uniqueDeployableActivated;
    private Image UDBindProgress;
    private int holdTimeForUD = 1;
    private float _udHoldProgress = 0f;
    [SerializeField] private UniqueDeployableMainScript _uniqueDeployableMainScript;

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

    public void ChangeWeapon()
    {
        secondaryAmmo.text = primaryAmmo.text + primaryAmmoReserve.text;
    }

    public bool IsUniqueDeployableActivated()
    {
        return uniqueDeployableActivated;
    }

    void UnequipToCasing()
    {
        // setactive false on all weapon models to "unequip" them, but keep them in inventory so they can be re-equipped when masking up
        for (int i = 0; i < inventory.Length; i++)
        {
            var inst = inventory[i];
            if (inst != null && inst.weaponModel != null)
                inst.weaponModel.SetActive(false);
        }
    }

    void EquipFromCasing()
    {
        // set active the primary weapon and then start to allow weapon switching.
        currentSlot = WeaponSlot.Primary;
        // activate primary model if present
        var primary = inventory[(int)WeaponSlot.Primary];
        if (primary != null && primary.weaponModel != null)
            primary.weaponModel.SetActive(true);

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

        if (StartingPrimary != null) EquipWeapon(WeaponSlot.Primary, StartingPrimary, StartingPrimary.model);
        if (StartingSecondary != null) EquipWeapon(WeaponSlot.Secondary, StartingSecondary, StartingSecondary.model);
        if (StartingGadget1 != null) EquipWeapon(WeaponSlot.Gadget1, StartingGadget1, StartingGadget1.model);
        if (StartingGadget2 != null) EquipWeapon(WeaponSlot.Gadget2, StartingGadget2, StartingGadget2.model);
        if (StartingGrenade != null) EquipWeapon(WeaponSlot.Grenade, StartingGrenade, StartingGrenade.model);
        if (StartingUniqueDeployable != null) Instantiate(StartingUniqueDeployable.uniqueDeployablePrefab, UDInsantObject.transform);

        UpdateWeaponNameText();
        ShowOnlyCurrentWeaponModel();

        secondaryAmmo.text = StartingSecondary.magazineSize.ToString() + "/" + StartingSecondary.reserveAmmo.ToString();
        GiveControllersBulletHolePrefabs();

        // initialize signature cooldown UI
        if (SignatureCooldown != null)
            SignatureCooldown.fillAmount = 1f; // ready

        // set hold time from unique deployable if provided
        if (StartingUniqueDeployable != null && StartingUniqueDeployable.equipTime > 0f)
        {
            holdTimeForUD = Mathf.Max(1, Mathf.RoundToInt(StartingUniqueDeployable.equipTime));
        }

        UDBindProgress = GameObject.FindWithTag("UDBindHold")?.GetComponent<Image>();
        _uniqueDeployableMainScript = GameObject.FindFirstObjectByType<UniqueDeployableMainScript>().GetComponent<UniqueDeployableMainScript>();

        // equip all the guns, but do not go into masked-up mode.
        // then unequiptocasing so the player has all the HUD assets actually showing,
        // then the player can maskup and equip the primary weapon to then begin shooting shit.


        // initialize HUD and controllers for guns, gadgets and grenade
        StartGuns();
        StartGadgets();
        StartGrenade();

        UnequipToCasing();
    }

    public void MaskUp()
    {
        // this is how the guns and stuff are enabled - meaning the game can start in a casing mode.
        MissionManager.Instance.currentPlayerState = MissionManager.PlayerState.Masked;

        EquipFromCasing();

        maskedUp = true;
    }

    private void Update()
    {
        if (!maskedUp)
            return; 

        // update signature cooldown timer and UI
        if (signatureCooldownRemaining > 0f)
        {
            signatureCooldownRemaining -= Time.deltaTime;
            if (signatureCooldownRemaining < 0f) signatureCooldownRemaining = 0f;
        }
        if (SignatureCooldown != null)
        {
            if (StartingSignature != null && StartingSignature.cooldownTime > 0)
            {
                float denom = StartingSignature.cooldownTime;
                // show progress filling up as cooldown replenishes
                float progress = 1f - (signatureCooldownRemaining / denom);
                SignatureCooldown.fillAmount = Mathf.Clamp01(progress);
            }
            else
            {
                SignatureCooldown.fillAmount = 1f;
            }
        }

        UpdateWeaponNameText();

        if (playerInput != null)
        {
            // weapon slot switching
            if (playerInput.actions["Primary"] != null && playerInput.actions["Primary"].WasPressedThisFrame())
                SwitchToSlot(WeaponSlot.Primary);
            else if (playerInput.actions["Secondary"] != null && playerInput.actions["Secondary"].WasPressedThisFrame())
                SwitchToSlot(WeaponSlot.Secondary);
            else if (playerInput.actions["Gadget1"] != null && playerInput.actions["Gadget1"].WasPressedThisFrame())
                SwitchToSlot(WeaponSlot.Gadget1);
            else if (playerInput.actions["Gadget2"] != null && playerInput.actions["Gadget2"].WasPressedThisFrame())
                SwitchToSlot(WeaponSlot.Gadget2);
            else if (playerInput.actions["Grenade"] != null && playerInput.actions["Grenade"].WasPressedThisFrame())
                SwitchToSlot(WeaponSlot.Grenade);

            // Signature button: tap = SignatureDeploy, hold = UniqueDeployable
            var sigAction = playerInput.actions["Signature"];
            if (sigAction != null)
            {
                if (sigAction.WasPressedThisFrame())
                {
                    _sigHoldInProgress = true;
                    _sigHoldTimer = 0f;
                    _sigHoldTriggered = false;
                    if (UDBindProgress != null)
                        UDBindProgress.fillAmount = 0f;
                }

                if (_sigHoldInProgress && sigAction.IsPressed())
                {
                    _sigHoldTimer += Time.deltaTime;
                    // update UDCooldown UI to show hold progress
                    float progress = Mathf.Clamp01(_sigHoldTimer / holdTimeForUD);
                    if (UDBindProgress != null)
                        UDBindProgress.fillAmount = progress;

                    if (!_sigHoldTriggered && _sigHoldTimer >= holdTimeForUD)
                    {
                        // trigger unique deploy
                        TryUniqueDeploy();
                        _sigHoldTriggered = true;
                        _sigHoldInProgress = false;
                    }
                }

                if (_sigHoldInProgress && sigAction.WasReleasedThisFrame())
                {
                    // tap: if hold duration less than threshold, treat as signature press
                    if (!_sigHoldTriggered)
                    {
                        SignatureDeploy();
                    }
                    _sigHoldInProgress = false;
                    _sigHoldTimer = 0f;
                    if (UDBindProgress != null) UDBindProgress.fillAmount = 0f;
                }
            }
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

    private void SignatureDeploy()
    {
        // check cooldown
        if (StartingSignature == null) return;
        if (signatureCooldownRemaining > 0f) return;

        if (SignatureDeployable != null)
        {
            // if there is already a deployed signature, destroy it before deploying a new one
            Destroy(SignatureDeployable);
        }

        // instantiate the signature deployable and throw it forward from the player's camera, the deployable's own script will handle its behavior and destruction
        if (StartingSignature != null && StartingSignature.model != null)
        {
            SignatureDeployable = Instantiate(StartingSignature.model, weaponHolder.transform.position, weaponHolder.transform.rotation);
            Rigidbody rb = SignatureDeployable.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Camera.main.transform.forward * 10f; // throw forward with some speed
            }
            // start cooldown
            signatureCooldownRemaining = Mathf.Max(0, StartingSignature.cooldownTime);
        }
    }

    private void TryUniqueDeploy()
    {
        if (StartingUniqueDeployable == null) return;

        // If not currently equipped, equip 
        if (!uniqueDeployableActivated)
        {
            if (_uniqueDeployableMainScript != null)
            {
                _uniqueDeployableMainScript.ToggleUniqueDeployable();
            }
        }
        else
        {
            if (_uniqueDeployableMainScript != null)
            {
                _uniqueDeployableMainScript.ToggleUniqueDeployable(); 
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
