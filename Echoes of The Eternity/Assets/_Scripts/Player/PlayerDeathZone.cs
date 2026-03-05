using UnityEngine;

public class PlayerDeathZone : MonoBehaviour
{
    [SerializeField] private bool isTrigger = true;
    private Collider playerCollider;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private int yThreshold = -10; // Set this to the desired Y-axis threshold for death

    private void OnTriggerEnter(Collider other)
    {
        if (!isTrigger) return;
        if (other.CompareTag("Player")) playerHealth.TakeDamage(playerHealth.MaxHealth);
    }

    private void Update()
    {
        if (isTrigger) return;
        if (playerHealth.transform.position.y < yThreshold) playerHealth.TakeDamage(playerHealth.MaxHealth);
    }
}
