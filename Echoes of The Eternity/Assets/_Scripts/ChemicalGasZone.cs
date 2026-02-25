using UnityEngine;

public class ChemicalGasZone : MonoBehaviour
{
    private bool gasZoneEnabled = false;
    [SerializeField] private float damagePerSecond = 10f; // Damage applied to the player per second while in the gas zone
    [SerializeField] private float gasDuration = 5f; // Duration the player can stay in the gas zone before taking damage
    [SerializeField] private ParticleSystem gasEffect; // Visual effect for the chemical gas
    [SerializeField] private SphereCollider gasCollider; // Collider representing the gas zone
    [SerializeField] private PlayerHealth _fpc; // Reference to the player's health component
    [SerializeField] private GasMaskScript _GMS;

    public void ToggleGasZone()
    {
        if (gasZoneEnabled)
        {
            gasZoneEnabled = false;
            gasEffect.Stop();

            gasCollider.enabled = false;
        }
        else
        {
            gasZoneEnabled = true;
            gasEffect.Play();
            gasCollider.enabled = true;
        }   
    }

    private void Awake()
    {
        _GMS = FindFirstObjectByType<GasMaskScript>();
        gasCollider.enabled = false; // Ensure the gas collider is initially disabled
        FindGasMask();
    }

    void FindGasMask()
    {
        _GMS = FindFirstObjectByType<GasMaskScript>();
    }

    private void Update()
    {
        if (gasZoneEnabled && gasCollider.enabled)
        {
            // Check if the player is within the gas zone
            if (gasCollider.bounds.Contains(_fpc.transform.position))
            {
                if (_GMS == null)
                {
                    FindGasMask();
                }
                if (!_GMS.IsGasMaskActive())
                {
                    // Apply full damage to the player over time
                    _fpc.TakeDamage(damagePerSecond * Time.deltaTime);
                } 
            }
        }
    }
}
