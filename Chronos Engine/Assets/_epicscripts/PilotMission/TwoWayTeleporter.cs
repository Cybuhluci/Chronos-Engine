using UnityEngine;
using System.Collections;

public class TwoWayTeleporter : MonoBehaviour
{
    [SerializeField] TwoWayTeleporter OtherTeleporter;
    [SerializeField] Transform BlueTeleporter;
    [SerializeField] Transform OrangeTeleporter;

    [Space(5)]
    [SerializeField] bool IsBlueTeleport;
    [SerializeField] float TeleportDelay = 0.5f;
    [SerializeField] float TeleportCooldown = 1.5f;

    [Space(5)]
    public bool IsActive = true;
    public bool CanTeleport = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsActive || !CanTeleport || !OtherTeleporter.CanTeleport) return;

        GameObject player = other.gameObject;
        StartCoroutine(BeginTeleport(player));
    }

    IEnumerator BeginTeleport(GameObject player)
    {
        CanTeleport = false;
        OtherTeleporter.CanTeleport = false;

        yield return new WaitForSeconds(TeleportDelay);

        Transform targetTransform = IsBlueTeleport ? OrangeTeleporter : BlueTeleporter;

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false; // disable to prevent blocking movement
        }

        player.transform.position = targetTransform.position;

        if (cc != null)
        {
            cc.enabled = true;
        }

        StartCoroutine(ReEnableTeleport());
        OtherTeleporter.StartCoroutine(OtherTeleporter.ReEnableTeleport());
    }

    IEnumerator ReEnableTeleport()
    {
        yield return new WaitForSeconds(TeleportCooldown);
        CanTeleport = true;
    }
}
