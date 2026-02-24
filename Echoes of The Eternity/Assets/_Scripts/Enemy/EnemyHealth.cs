using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class EnemyHealth : MonoBehaviour
{
    public EnemyStatsSO Stats;

    public bool IsAlive { get; private set; } = true;
    public float CurrentHealth { get; private set; }

    private void Awake()
    {
        if (Stats == null) Debug.LogWarning($"{name} has no Stats assigned.");
        CurrentHealth = Stats != null ? Stats.MaxHealth : 100f;
    }

    public void TakeDamage(float amount, GameObject source = null)
    {
        if (!IsAlive) return;

        CurrentHealth -= amount;

        // Optional knockback if source has rigidbody:
        if (source != null && Stats != null)
        {
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 dir = (transform.position - source.transform.position).normalized;
                rb.AddForce(dir * Stats.KnockbackForce, ForceMode.Impulse);
            }
        }

        if (CurrentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (!IsAlive) return;
        CurrentHealth = Mathf.Min(CurrentHealth + amount, Stats != null ? Stats.MaxHealth : CurrentHealth);
    }

    private void Die()
    {
        IsAlive = false;
        // disable collider so no more interactions
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // sample: disable AI component(s) + schedule destroy
        var ai = GetComponent<EnemyController>();
        if (ai != null) ai.OnDeath();

        float delay = Stats != null ? Stats.DeathDelay : 5f;
        Destroy(gameObject, delay);
    }
}
