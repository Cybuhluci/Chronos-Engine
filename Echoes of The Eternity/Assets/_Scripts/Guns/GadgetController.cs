using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GadgetController : MonoBehaviour
{
    public GadgetDataSO gadgetData; // Reference to the ScriptableObject containing the gadget's data
    [SerializeField] int currentAmmo; // Current ammo count for the gadget
    [SerializeField] Image chunkImage; // Reference to the UI image representing the ammo chunk
    [SerializeField] TMP_Text gadgetAmmo;

    [SerializeField] GameObject UIInstanceGadget3, UIInstanceGadget4;
    [SerializeField] GameObject UIInstanceContainer;
    [Header("Fire Settings")]
    [Tooltip("Maximum ray distance for gadget fire")]
    [SerializeField] private float fireRange = 60f;
    [Tooltip("Radius of the area damage sphere at hit point")]
    [SerializeField] private float explosionRadius = 3f;
    [Tooltip("Force applied to nearby rigidbodies on hit")]
    [SerializeField] private float explosionForce = 300f;
    [Tooltip("Layer mask for damage checks (optional)")]
    [SerializeField] private LayerMask hitMask = ~0;

    [SerializeField] private PlayerInput playerInput;

    public void StartGadget()
    {
        currentAmmo = gadgetData.ammoStart; // Initialize current ammo to max ammo from the ScriptableObject

        UIInstanceContainer = GameObject.FindWithTag("WeaponInstantHUD");

        if (UIInstanceContainer != null && (UIInstanceGadget3 != null || UIInstanceGadget4 != null))
        {
            // find a way to make it so that it checks if gadget 3 is already used, then uses gadget 4.
            if (GameObject.FindWithTag("Gadget3") == null)
            {
                GameObject instantiatedUI = Instantiate(UIInstanceGadget3, UIInstanceContainer.transform);
                gadgetAmmo = instantiatedUI.GetComponentInChildren<TMP_Text>();
                chunkImage = instantiatedUI.GetComponentInChildren<Image>();
            }
            else
            {
                GameObject instantiatedUI = Instantiate(UIInstanceGadget4, UIInstanceContainer.transform);
                gadgetAmmo = instantiatedUI.GetComponentInChildren<TMP_Text>();
                chunkImage = instantiatedUI.GetComponentInChildren<Image>();
            }
        }

        playerInput = GameObject.FindWithTag("Player").GetComponent<PlayerInput>();

        UpdateAmmoUI();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAmmoUI();

        if (playerInput.actions["Fire"].WasPerformedThisFrame())
        {
            Fire();
        }
    }

    public void UpdateAmmoUI()
    {
        if (gadgetAmmo != null)
        {
            gadgetAmmo.text = currentAmmo.ToString();
        }
    }

    public void FillChunkAmmo(float percentage)
    {
        // fills the chunkImage, when the image is full, adds 1 ammo.
        if (chunkImage != null)
        {
            chunkImage.fillAmount += percentage;
            if (chunkImage.fillAmount >= 1f)
            {
                chunkImage.fillAmount = 0f;
                currentAmmo++;
            }
        }
    }

    // Simple fire method: raycasts from the camera forward; at the hit point it performs an overlap sphere
    // and deals damage to any EnemyHealth components found. Also applies physics impulse to rigidbodies.
    public void Fire()
    {
        // consume one ammo if available
        if (currentAmmo <= 0) return;
        currentAmmo--;

        Camera cam = Camera.main;
        Vector3 origin = cam != null ? cam.transform.position : transform.position;
        Vector3 dir = cam != null ? cam.transform.forward : transform.forward;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, fireRange, hitMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 center = hit.point;
            // overlap to find targets
            Collider[] cols = Physics.OverlapSphere(center, explosionRadius, hitMask, QueryTriggerInteraction.Ignore);
            foreach (var col in cols)
            {
                if (col == null) continue;
                var eh = col.GetComponentInParent<EnemyHealth>();
                if (eh != null)
                {
                    float dmg = (gadgetData != null) ? gadgetData.damage : 10f;
                    eh.TakeDamage(dmg, gameObject);
                }

                // apply physics impulse
                var rb = col.attachedRigidbody;
                if (rb != null && !rb.isKinematic)
                {
                    rb.AddExplosionForce(explosionForce, center, explosionRadius, 1f, ForceMode.Impulse);
                }
            }
        }
        else
        {
            // nothing hit; optional: still apply a small area in front of the player
            Vector3 center = origin + dir * (fireRange * 0.5f);
            Collider[] cols = Physics.OverlapSphere(center, explosionRadius, hitMask, QueryTriggerInteraction.Ignore);
            foreach (var col in cols)
            {
                if (col == null) continue;
                var eh = col.GetComponentInParent<EnemyHealth>();
                if (eh != null)
                {
                    float dmg = (gadgetData != null) ? gadgetData.damage : 10f;
                    eh.TakeDamage(dmg, gameObject);
                }
                var rb = col.attachedRigidbody;
                if (rb != null && !rb.isKinematic)
                {
                    rb.AddExplosionForce(explosionForce * 0.5f, center, explosionRadius, 0.5f, ForceMode.Impulse);
                }
            }
        }
    }
}
