using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Splines;

public class GunController : MonoBehaviour
{
    public MainGunDataSO gunData; // Assign in prefab or at runtime

    public int currentAmmo;
    public int reserveAmmo;
    public float fireCooldown = 0f;
    public bool isBursting = false;
    public bool triggerReleasedSinceLastShot = true;

    [SerializeField] private AudioSource audioSource; // Assign in inspector
    [SerializeField] private AudioClip[] fireSounds; // Assign in inspector

    [SerializeField] private TMP_Text primaryAmmo, primaryAmmoReserve; // technically "held weapon ammo"

    public void UpdateAmmoUI()
    {
        if (primaryAmmo != null)
        {
            primaryAmmo.text = $"{currentAmmo}";
        }
        if (primaryAmmoReserve != null)
        {
            primaryAmmoReserve.text = $"/{reserveAmmo}";
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

    [Header("Recoil")]
    [Tooltip("Transform to apply recoil to (usually the camera or weapon) ")]
    [SerializeField] private Transform recoilTarget;
    [Tooltip("How strong the recoil impulse is (degrees)")]
    [SerializeField] private float recoilStrength = 2.5f;
    [Tooltip("How quickly recoil recovers back to neutral")]
    [SerializeField] private float recoilRecoverSpeed = 8f;
    [Tooltip("Multiplier applied to the vertical component of recoil (uses gunData.recoil as base degrees per shot)")]
    [SerializeField] private float verticalRecoilMultiplier = 1f;
    [Tooltip("Multiplier applied to the horizontal/random component of recoil")]
    [SerializeField] private float horizontalRecoilMultiplier = 0.15f;
    [Tooltip("When aiming down sights, recoil is multiplied by this (values <1 reduce recoil)")]
    [SerializeField] private float adsRecoilMultiplier = 0.5f;
    [Tooltip("Clamp total recoil angle (degrees) to avoid extreme flipping)")]
    [SerializeField] private float maxRecoilAngle = 25f;
    [Tooltip("Global scale applied to recoil angles added per shot (use to tune kickback without changing SO values)")]
    [SerializeField] private float recoilAngleScale = 1f;

    [Header("Recoil Calibration")]
    private float baselineRecoilValue = 100f; 
    // Reference recoil value in data that represents a baseline weapon (e.g. 100 = Colt 1911)
    [Tooltip("Vertical recoil in degrees applied when recoil equals baselineRecoilValue")]
    [SerializeField] private float baselineVerticalDegrees = 2f;
    [Tooltip("Horizontal recoil as fraction of vertical at baseline (e.g. 0.15 = 15% of vertical)")]
    [SerializeField] private float baselineHorizontalFraction = 0.15f;
    // whether the player is aiming down sights (reduces recoil and spread)
    private bool isAiming = false;

    // camera recoil settings
    private Transform playerCameraTransform; 
    // Assign in Start by finding camera with tag (e.g. CinemachineTarget)
    private float cameraRecoilPositionFactor = 0.025f; 
    // How much positional kick the camera receives per degree of vertical recoil
    private float cameraRecoilRotationFactor = 1; 
    // How much rotational kick (degrees) the camera receives per degree of vertical recoil
    private float cameraRecoilRecoverSpeed = 5f; 
    // How quickly the camera recovers from recoil (seconds) - larger is slower
    private float cameraRecoilScale = 10f; 
    // Global scale applied to camera recoil impulses (pos/rot). Use <1 to reduce visible kick)
    private float maxCameraPosKick = 0.1f; 
    // Clamp maximum camera positional kick magnitude per shot
    private float maxCameraRotKick = 100f; 
    //Clamp maximum camera rotational kick (degrees) per shot

    // camera recoil runtime state
    private Vector3 cameraRecoilPosVelocity = Vector3.zero;
    private Vector3 cameraRecoilRotVelocity = Vector3.zero;
    private Vector3 currentCameraRecoilPos = Vector3.zero;
    private Vector3 currentCameraRecoilEuler = Vector3.zero;
    private Vector3 targetCameraRecoilPos = Vector3.zero;
    private Vector3 targetCameraRecoilEuler = Vector3.zero;
    private Vector3 originalCameraLocalPosition = Vector3.zero;
    private Quaternion originalCameraLocalRotation = Quaternion.identity;
    // track last applied recoil so we can apply deltas on top of whatever the camera currently does
    private Vector3 lastAppliedCameraRecoilPos = Vector3.zero;
    private Vector3 lastAppliedCameraRecoilEuler = Vector3.zero;
    private Transform lastAppliedCameraTransform = null;

    // internal recoil state
    private Vector2 recoilVelocity = Vector2.zero;
    private Vector2 recoilAngle = Vector2.zero; // x = pitch, y = yaw
    private Quaternion originalRecoilLocalRotation = Quaternion.identity;

    public void StartGun()
    {
        playerCameraTransform = GameObject.FindWithTag("CinemachineTarget")?.transform; ; // doesnt work, as it is the par
        primaryAmmo = GameObject.FindWithTag("PrimaryAmmoMagazine")?.GetComponent<TMP_Text>();
        primaryAmmoReserve = GameObject.FindWithTag("PrimaryAmmoReserve")?.GetComponent<TMP_Text>();

        if (gunData != null)
        {
            currentAmmo = gunData.magazineSize;
            reserveAmmo = gunData.reserveAmmo;
        }

        // Cache the original local rotation of the recoil target so recoil is applied as an offset
        if (recoilTarget != null)
        {
            originalRecoilLocalRotation = recoilTarget.localRotation;
        }

        // Reset last-applied tracking when camera found
        if (playerCameraTransform != null)
        {
            lastAppliedCameraRecoilPos = Vector3.zero;
            lastAppliedCameraRecoilEuler = Vector3.zero;
            lastAppliedCameraTransform = playerCameraTransform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;

        // Smooth recoil recovery
        if (recoilTarget != null)
        {
            // Smoothly reduce recoilAngle towards zero
            recoilAngle = Vector2.SmoothDamp(recoilAngle, Vector2.zero, ref recoilVelocity, 1f / Mathf.Max(0.001f, recoilRecoverSpeed));
            // Apply pitch (x) and yaw (y) as an offset from the original local rotation
            Quaternion recoilOffset = Quaternion.Euler(-recoilAngle.x, recoilAngle.y, 0f);
            recoilTarget.localRotation = originalRecoilLocalRotation * recoilOffset;
        }

        // Smooth camera recoil recovery (compute current recoil offsets; actual application happens in LateUpdate)
        if (playerCameraTransform != null)
        {
            // If the camera reference changed (e.g. reparented), reset last-applied tracking
            if (lastAppliedCameraTransform != playerCameraTransform)
            {
                lastAppliedCameraRecoilPos = Vector3.zero;
                lastAppliedCameraRecoilEuler = Vector3.zero;
                lastAppliedCameraTransform = playerCameraTransform;
            }

            // Smoothly move current towards target
            currentCameraRecoilPos = Vector3.SmoothDamp(currentCameraRecoilPos, targetCameraRecoilPos, ref cameraRecoilPosVelocity, 1f / Mathf.Max(0.001f, cameraRecoilRecoverSpeed));
            currentCameraRecoilEuler = Vector3.SmoothDamp(currentCameraRecoilEuler, targetCameraRecoilEuler, ref cameraRecoilRotVelocity, 1f / Mathf.Max(0.001f, cameraRecoilRecoverSpeed));

            // Gradually return the target recoil towards zero so it recovers over time
            targetCameraRecoilPos = Vector3.Lerp(targetCameraRecoilPos, Vector3.zero, Time.deltaTime * cameraRecoilRecoverSpeed);
            targetCameraRecoilEuler = Vector3.Lerp(targetCameraRecoilEuler, Vector3.zero, Time.deltaTime * cameraRecoilRecoverSpeed);
        }

        // Always update triggerReleasedSinceLastShot at the end
        triggerReleasedSinceLastShot = true;

        UpdateAmmoUI();
    }

    // Apply camera recoil offsets after other camera updates
    void LateUpdate()
    {
        if (playerCameraTransform == null) return;

        // compute the deltas between newly computed current recoil and last applied recoil
        Vector3 posDelta = currentCameraRecoilPos - lastAppliedCameraRecoilPos;
        Vector3 eulerDelta = currentCameraRecoilEuler - lastAppliedCameraRecoilEuler;

        // apply additive changes in local space
        playerCameraTransform.localPosition += posDelta;
        playerCameraTransform.localRotation *= Quaternion.Euler(eulerDelta);

        // remember what we applied so next frame we only add the delta
        lastAppliedCameraRecoilPos = currentCameraRecoilPos;
        lastAppliedCameraRecoilEuler = currentCameraRecoilEuler;
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

        // Compute recoil based solely on gunData.recoil relative to a baseline value.
        float dataRecoil = gunData != null ? gunData.recoil : recoilStrength;
        float aimMultiplier = isAiming ? adsRecoilMultiplier : 1f;

        // Normalized factor where 1 == baselineRecoilValue
        float normalized = (baselineRecoilValue > 0f) ? (dataRecoil / baselineRecoilValue) : 1f;

        // Vertical degrees to apply this shot (scaled by ADS)
        float verticalImpulse = normalized * baselineVerticalDegrees * aimMultiplier;

        // Horizontal is a random +/- fraction of the baseline vertical degrees
        float horizontalRange = baselineVerticalDegrees * baselineHorizontalFraction * normalized * aimMultiplier;
        float horizontalImpulse = Random.Range(-horizontalRange, horizontalRange);

        // Apply impulses directly (no extra global scaling) to keep gunData authoritative
        recoilAngle.x += verticalImpulse; // pitch up
        recoilAngle.y += horizontalImpulse; // yaw

        // clamp recoil to avoid extreme angles
        recoilAngle.x = Mathf.Clamp(recoilAngle.x, 0f, maxRecoilAngle);
        recoilAngle.y = Mathf.Clamp(recoilAngle.y, -maxRecoilAngle, maxRecoilAngle);

        fireCooldown = 60f / Mathf.Max(0.0001f, gunData != null ? gunData.fireRate : 600f); // fireRate is rounds per minute
        currentAmmo--;

        // Apply camera recoil impulse (positional and rotational) based on vertical recoil
        if (playerCameraTransform != null)
        {
            float camPosKick = verticalImpulse * cameraRecoilPositionFactor * cameraRecoilScale;
            float camRotKick = verticalImpulse * cameraRecoilRotationFactor * cameraRecoilScale;

            // clamp the camera impulses so a single large recoil cannot be too extreme
            camPosKick = Mathf.Clamp(camPosKick, -maxCameraPosKick, maxCameraPosKick);
            camRotKick = Mathf.Clamp(camRotKick, -maxCameraRotKick, maxCameraRotKick);

            // camera kick back along its local -Z
            targetCameraRecoilPos += -playerCameraTransform.forward * camPosKick;
            // camera pitch upwards (negative pitch) on X axis
            targetCameraRecoilEuler += new Vector3(-camRotKick, Random.Range(-camRotKick * 0.2f, camRotKick * 0.2f), 0f);
        }
    }

    private IEnumerator BurstFire()
    {
        isBursting = true;
        int burstCount = gunData.bulletsPerShot > 1 ? gunData.bulletsPerShot : 3; // Default to 3-round burst if not set
        float burstDelay = 60f / Mathf.Max(0.0001f, gunData.fireRate);

        for (int i = 0; i < burstCount; i++)
        {
            if (currentAmmo > 0)
            {
                Attack();
                PlayerGunSounds();
                // small recoil per burst shot (scaled down)
                float baseRecoilBurst = gunData != null ? gunData.recoil * 0.5f : recoilStrength * 0.5f;
                float aimMultiplierBurst = isAiming ? adsRecoilMultiplier : 1f;
                float verticalBurst = baseRecoilBurst * verticalRecoilMultiplier * aimMultiplierBurst;
                float horizontalBurst = Random.Range(-baseRecoilBurst * horizontalRecoilMultiplier, baseRecoilBurst * horizontalRecoilMultiplier) * aimMultiplierBurst;
                recoilAngle.x += verticalBurst * recoilAngleScale;
                recoilAngle.y += horizontalBurst * recoilAngleScale;
                recoilAngle.x = Mathf.Clamp(recoilAngle.x, 0f, maxRecoilAngle);
                recoilAngle.y = Mathf.Clamp(recoilAngle.y, -maxRecoilAngle, maxRecoilAngle);

                // camera recoil for burst shot
                if (playerCameraTransform != null)
                {
                    float camPosKickB = verticalBurst * cameraRecoilPositionFactor * cameraRecoilScale;
                    float camRotKickB = verticalBurst * cameraRecoilRotationFactor * cameraRecoilScale;
                    camPosKickB = Mathf.Clamp(camPosKickB, -maxCameraPosKick, maxCameraPosKick);
                    camRotKickB = Mathf.Clamp(camRotKickB, -maxCameraRotKick, maxCameraRotKick);
                    targetCameraRecoilPos += -playerCameraTransform.forward * camPosKickB;
                    targetCameraRecoilEuler += new Vector3(-camRotKickB, Random.Range(-camRotKickB * 0.2f, camRotKickB * 0.2f), 0f);
                }
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

        int pellets = Mathf.Max(1, gunData.bulletsPerShot);
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

    public void ReplenishAmmo(int percentage)
    {
        if (gunData == null) return;
        int amount = Mathf.CeilToInt(gunData.reserveAmmo * (percentage / 100f));
        reserveAmmo = Mathf.Min(reserveAmmo + amount, gunData.reserveAmmo);
    }
}
