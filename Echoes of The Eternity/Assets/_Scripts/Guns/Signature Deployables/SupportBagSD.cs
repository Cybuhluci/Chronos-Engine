using TMPro;
using UnityEngine;

public class SupportBagSD : MonoBehaviour
{
    [SerializeField] SignatureDeployableSO deployableData; // reference to the scriptable object containing the deployable's data, such as ammo and health values.
    public Rigidbody rb; // rigidbody of the bag, used for physics interactions and movement.
    public SphereCollider pickupDist; // the collider used to detect and give health and ammo to the player.
    public BoxCollider bagCollider; // the bounding box of the bag, used for interactions and location detection or whatever.

    public bool _isBagDeployed; // whether the bag is currently active or not (as in falling or stationary on the floor).
    public bool _isGrounded; // whether the bag is currently touching the ground or not, used to determine if the bag is deployed or not.
    public float groundCheck = 1f; // whether the bag is currently checking if it is on the ground or not, used to prevent multiple checks at once.

    [SerializeField] private TMP_Text AmmoCounter;
    [SerializeField] private int currentAmmo;

    private void Awake()
    {
        AmmoCounter = GameObject.FindWithTag("SignatureDeployAmmo")?.GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (!_isGrounded)
        {
            _isBagDeployed = false; // if the bag is not grounded, it is not deployed.
            bagCollider.isTrigger = false; // make the bag collider a trigger so it does not interfere with physics while falling.
            CheckIfOnFloor();
        }
        else
        {
            GiveAmmoAndHealth();
        }

        AmmoCounter.text = currentAmmo.ToString();
    }

    private void CheckIfOnFloor()
    {
        // Prefer using the bag collider for ground checks so pickupDist does not interfere
        RaycastHit hit;
        if (bagCollider != null)
        {
            // bottom point of the bag collider
            Vector3 bottom = bagCollider.bounds.center - Vector3.up * bagCollider.bounds.extents.y;
            // start slightly above the bottom to avoid immediate self-hit
            Vector3 start = bottom + Vector3.up * 0.05f;
            float maxDistance = groundCheck + 0.05f;

            if (Physics.Raycast(start, Vector3.down, out hit, maxDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider != null && hit.collider.CompareTag("Ground"))
                {
                    _isGrounded = true;
                    _isBagDeployed = true;
                    bagCollider.isTrigger = true;

                    // stop motion
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                        rb.isKinematic = true;
                    }

                    // place the bag so its bottom touches the hit point and align to the surface normal
                    float halfHeight = bagCollider.bounds.extents.y;
                    Vector3 desiredPos = hit.point + hit.normal * halfHeight;
                    transform.position = desiredPos;

                    // build a forward vector projected onto the hit plane to avoid twisting
                    Vector3 projectedForward = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;
                    if (projectedForward.sqrMagnitude < 0.0001f)
                    {
                        // fallback: use transform.up projected
                        projectedForward = Vector3.ProjectOnPlane(transform.up, hit.normal).normalized;
                        if (projectedForward.sqrMagnitude < 0.0001f)
                            projectedForward = Vector3.Cross(hit.normal, Vector3.up).normalized;
                    }

                    transform.rotation = Quaternion.LookRotation(projectedForward, hit.normal);
                }
            }
        }
        else
        {
            // fallback to a simple spherecast when no bag collider is assigned
            if (Physics.SphereCast(transform.position, groundCheck, Vector3.down, out hit, groundCheck))
            {
                if (hit.collider != null && hit.collider.CompareTag("Ground"))
                {
                    _isGrounded = true;
                    _isBagDeployed = true;
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                        rb.isKinematic = true;
                    }
                    // attempt to align upright using the hit normal
                    Vector3 forward = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;
                    if (forward.sqrMagnitude < 0.0001f) forward = Vector3.Cross(hit.normal, Vector3.up).normalized;
                    transform.rotation = Quaternion.LookRotation(forward, hit.normal);
                }
            }
        }
    }

    private void GiveAmmoAndHealth()
    {
        // give the player ammo and health when they are within the pickup distance of the bag, and the bag is deployed.
        if (_isBagDeployed)
        {
            // check if the player is within the pickup distance of the bag, if they are, give them ammo and health.
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, pickupDist.radius);
            foreach (Collider hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    // give the player ammo and health here.
                }
            }
        }
    }

    // Visualize the ground-check spherecast and the pickup radius in the scene view
    private void OnDrawGizmosSelected()
    {
        // origin and parameters may be null in edit mode; guard them
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.6f); // orange for ground check

        if (bagCollider != null)
        {
            Vector3 bottom = bagCollider.bounds.center - Vector3.up * bagCollider.bounds.extents.y;
            Vector3 start = bottom + Vector3.up * 0.05f;
            Vector3 end = start + Vector3.down * (groundCheck + 0.05f);

            // draw start and end small spheres and connecting line
            Gizmos.DrawWireSphere(start, 0.02f);
            Gizmos.DrawWireSphere(end, 0.02f);
            Gizmos.DrawLine(start, end);

            // draw a box representing the bag collider bounds
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.1f);
            Gizmos.DrawCube(bagCollider.bounds.center, bagCollider.bounds.size);
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.6f);
            Gizmos.DrawWireCube(bagCollider.bounds.center, bagCollider.bounds.size);
        }
        else
        {
            Vector3 origin = transform.position;
            float sphereRadius = Mathf.Max(0.001f, groundCheck);
            float castDistance = groundCheck;
            Gizmos.DrawWireSphere(origin, sphereRadius);
            Vector3 endCenter = origin + Vector3.down * castDistance;
            Gizmos.DrawWireSphere(endCenter, sphereRadius);
            Gizmos.DrawLine(origin, endCenter);
        }

        // draw pickup radius in green (if available)
        if (pickupDist != null)
        {
            Vector3 origin = transform.position;
            float pickupRadius = pickupDist.radius;
            Gizmos.color = new Color(0f, 1f, 0f, 0.15f);
            Gizmos.DrawSphere(origin, pickupRadius);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(origin, pickupRadius);
        }
    }
}
