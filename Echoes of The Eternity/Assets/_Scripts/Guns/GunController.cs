using Luci;
using System.Collections;
using TMPro;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public MainGunDataSO gunData; // Assign in prefab or at runtime
    public InventoryManager inventoryManager; // Assign in inspector or at runtime
    public FirstPersonController playerController;

    public int overallAmmo;
    public int currentAmmo;
    public int reserveAmmo;
    public string ammoType = ""; // e.g. "HP", "AP", "SLG", "+P", etc. for display purposes; no functional effect in this demo
    public float fireCooldown = 0f;
    public bool isBursting = false;
    public bool triggerReleasedSinceLastShot = true;

    [SerializeField] private AudioSource audioSource; // Assign in inspector
    [SerializeField] private AudioClip[] fireSounds; // Assign in inspector

    [SerializeField] private TMP_Text gunAmmo, gunAmmoType; // technically "held weapon ammo"

    public void UpdateAmmoUI()
    {
        if (gunAmmo != null)
        {
            gunAmmo.text = $"{currentAmmo}/{reserveAmmo}";
        }
        if (gunAmmoType != null)
        {
            gunAmmoType.text = $"{gunData.ammoType.name}";
        }
    }

    public void SetBulletHolePrefabs(GameObject woodPrefab, GameObject metalPrefab, GameObject concretePrefab)
    {
        WoodPrefab = woodPrefab;
        MetalPrefab = metalPrefab;
        ConcretePrefab = concretePrefab;
    }

    [Header("Ballistics / Effects")]
    [Tooltip("Maximum range for hitscan bullets")] [SerializeField] private float range = 200f;
    [Tooltip("Layer mask for bullet raycasts")] [SerializeField] private LayerMask hitMask = ~0;
    private GameObject WoodPrefab, MetalPrefab, ConcretePrefab;
    //Prefab to spawn at impact points
    [Tooltip("Muzzle flash particle system (optional)")] [SerializeField] private ParticleSystem muzzleFlash;

    // Recoil removed: starting fresh. Weapon will still fire but no positional/rotational recoil is applied here.
    private bool isAiming = false;
    [Header("Sprinting Animation")]
    [Tooltip("Enable sprinting weapon pose")]
    public bool enableSprintingPose = true;
    [Tooltip("Local position offset applied to weapon while sprinting (relative to original local position)")]
    public Vector3 sprintLocalPosition = new Vector3(0f, -0.15f, -0.2f);
    [Tooltip("Local euler rotation applied to weapon while sprinting")]
    public Vector3 sprintLocalEuler = new Vector3(-25f, 0f, 0f);
    [Tooltip("How quickly the weapon blends to/from sprint pose")]
    public float sprintBlendSpeed = 8f;

    // runtime original transforms for blending
    private Vector3 _originalLocalPosForSprint;
    private Quaternion _originalLocalRotForSprint;
    private bool _sprintTransformsInitialized = false;

    public void StartGun(GunMainScript gms)
    {
        // find HUD elements
        gunAmmo = gms.gunAmmo;
        gunAmmoType = gms.gunAmmoType;

        inventoryManager = FindAnyObjectByType<InventoryManager>();
        playerController = FindAnyObjectByType<FirstPersonController>();

        if (gunData != null)
        {
            overallAmmo = inventoryManager.GetAmmoTypeAmount(gunData.ammoType);
            reserveAmmo = overallAmmo;
            currentAmmo = Mathf.Min(gunData.magazineSize, reserveAmmo);
            reserveAmmo -= currentAmmo;
        }

        // cache original local transform for sprinting animation
        _originalLocalPosForSprint = transform.localPosition;
        _originalLocalRotForSprint = transform.localRotation;
        _sprintTransformsInitialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;

        // Always update triggerReleasedSinceLastShot at the end
        triggerReleasedSinceLastShot = true;

        UpdateAmmoUI();
        SprintingAnimation();
    }

    // Allow other systems to set aiming state
    public void SetAiming(bool aiming)
    {
        isAiming = aiming;
    }

    // Call this from GunMainScript, passing fire input state
    public void TryFire(bool fireInput, bool fireInputDown)
    {
        if (gunData == null || isBursting) return;

        switch (gunData.fireMode)
        {
            case MainGunDataSO.FireMode.SemiAuto:
                if (fireInputDown && triggerReleasedSinceLastShot)
                {
                    FireOnce();
                    triggerReleasedSinceLastShot = false;
                }
                break;
            case MainGunDataSO.FireMode.FullAuto:
                if (fireInput)
                {
                    FireOnce();
                }
                break;
            case MainGunDataSO.FireMode.Burst:
                if (fireInputDown)
                {
                    StartCoroutine(BurstFire());
                }
                break;
        }
    }

    private void FireOnce()
    {
        if (fireCooldown > 0f || currentAmmo <= 0)
            return;

        Attack();
        PlayerGunSounds();

        // Play muzzle flash
        if (muzzleFlash != null)
            muzzleFlash.Play();

        // simple cooldown and ammo handling
        fireCooldown = 60f / Mathf.Max(0.0001f, gunData != null ? gunData.fireRate : 600f); // fireRate is rounds per minute
        currentAmmo--;
    }

    private IEnumerator BurstFire()
    {
        isBursting = true;
        int burstCount = gunData.burstAmount > 1 ? gunData.burstAmount : 3; // Default to 3-round burst if not set
        float burstDelay = 60f / Mathf.Max(0.0001f, gunData.fireRate);

        for (int i = 0; i < burstCount; i++)
        {
            if (currentAmmo > 0)
            {
                Attack();
                PlayerGunSounds();
                currentAmmo--;
            }
            else
            {
                break;
            }
            if (i < burstCount - 1)
                yield return new WaitForSeconds(burstDelay);
        }
        fireCooldown = burstDelay; // Add a delay after burst
        isBursting = false;
    }

    private void PlayerGunSounds()
    {
        if (audioSource != null && fireSounds.Length > 0)
        {
            AudioClip clip = fireSounds[Random.Range(0, fireSounds.Length)];
            audioSource.PlayOneShot(clip);
        }
    }

    private void Attack()
    {
        if (gunData == null) return;

        //remove amount of bullets per shot from current ammo, but ensure it doesn't go negative (for shotguns or multi-pellet weapons)
        inventoryManager.ConsumeAmmo(gunData.ammoType, gunData.ammoType.bulletsPerShot);

        int pellets = Mathf.Max(1, gunData.ammoType.bulletsPerShot);
        Camera cam = Camera.main;
        Vector3 origin = (cam != null) ? cam.transform.position : transform.position;
        Vector3 forward = (cam != null) ? cam.transform.forward : transform.forward;

        float spreadAngle = gunData.spread; // use recoil as simple spread value in degrees

        for (int i = 0; i < pellets; i++)
        {
            // random spread
            Vector3 dir = forward;
            if (spreadAngle > 0f)
            {
                float yaw = Random.Range(-spreadAngle, spreadAngle);
                float pitch = Random.Range(-spreadAngle, spreadAngle);
                dir = Quaternion.Euler(pitch, yaw, 0f) * forward;
            }

            if (Physics.Raycast(origin, dir, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
            {
                // spawn impact effect
                if (MetalPrefab != null)
                {
                    Instantiate(MetalPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                }

                // apply damage via SendMessage to allow flexible targets
                float damage = gunData.damage;
                // crude headshot detection by tag
                if (hit.collider != null && hit.collider.CompareTag("Head"))
                {
                    damage *= gunData.headshotMultiplier;
                }

                // Prefer EnemyHealth component when present
                var enemyHealth = hit.collider.GetComponentInParent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage, gameObject);
                }

                var camera = hit.collider.GetComponent<CameraDetection>();
                if (camera != null)
                {
                    camera.GetShot();
                }

                // Apply physical impulse if the hit object has a rigidbody
                if (hit.rigidbody != null)
                {
                    float impulseScale = (gunData != null) ? gunData.muzzleVelocity * 0.02f : 1f;
                    hit.rigidbody.AddForceAtPosition(dir * impulseScale, hit.point, ForceMode.Impulse);
                }

                Debug.Log($"Hit {hit.collider.name} for {damage} damage");
            }
            else
            {
                // miss
            }
        }
    }

    private void OnEnable()
    {
        // this reset doesnt work.
        transform.localPosition = transform.localPosition;
        transform.localRotation = transform.localRotation;
    }

    public void SprintingAnimation()
    {
        if (!enableSprintingPose || !_sprintTransformsInitialized) return;

        Vector3 targetPos = _originalLocalPosForSprint;
        Quaternion targetRot = _originalLocalRotForSprint;

        if (playerController != null && playerController.isSprinting)
        {
            targetPos = _originalLocalPosForSprint + sprintLocalPosition;
            targetRot = _originalLocalRotForSprint * Quaternion.Euler(sprintLocalEuler);
        }

        // blend towards target smoothly
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * sprintBlendSpeed);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime * sprintBlendSpeed);
    }

    public int GetCurrentAmmo() => currentAmmo;
    public int GetMaxAmmo() => gunData != null ? gunData.magazineSize : 0;
    public int GetReserveAmmo() => reserveAmmo;

    public void Reload()
    {
        if (gunData == null) return;
        int needed = gunData.magazineSize - currentAmmo;
        int toReload = Mathf.Min(needed, reserveAmmo);
        if (toReload > 0)
        {
            currentAmmo += toReload;
            reserveAmmo -= toReload;
        }
    }
}
