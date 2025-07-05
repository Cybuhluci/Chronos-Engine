using UnityEngine;

public class VolFirstPerson : MonoBehaviour
{
    public PerspectiveManager PlayerManager;

    private void OnTriggerEnter(Collider other)
    {
        PlayerManager = other.GetComponentInParent<PerspectiveManager>();

        PlayerManager.ToggleToFirstPerson();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
