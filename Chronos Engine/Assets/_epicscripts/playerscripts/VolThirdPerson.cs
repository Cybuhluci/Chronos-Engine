using UnityEngine;

public class VolThirdPerson : MonoBehaviour
{
    public PerspectiveManager PlayerManager;

    private void OnTriggerEnter(Collider other)
    {
        PlayerManager = other.GetComponentInParent<PerspectiveManager>();

        PlayerManager.ToggleToThirdPerson();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
