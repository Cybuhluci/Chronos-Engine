using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolSecondPerson : MonoBehaviour
{
    public PerspectiveManager PlayerManager;

    private void OnTriggerEnter(Collider other)
    {
        PlayerManager = other.GetComponentInParent<PerspectiveManager>();

        PlayerManager.ToggleToSecondPerson();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
