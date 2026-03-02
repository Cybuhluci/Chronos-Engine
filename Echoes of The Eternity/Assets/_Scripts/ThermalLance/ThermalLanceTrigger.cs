using Luci.Interactions;
using UnityEngine;

public class ThermalLanceTrigger : MonoBehaviour
{
    [SerializeField] private Collider _collider;
    [SerializeField] private Transform _transform;
    [SerializeField] private BasicKeyDoorScript _vaultDoor;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ThermalLance"))
        {
            GameObject drill = Instantiate(other.GetComponent<ThermalLance>().theDrill, _transform.position, _transform.rotation);
            drill.GetComponent<ThermalLance>().ActivateLance(_transform, _vaultDoor);
            _collider.enabled = false;
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}
