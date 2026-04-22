using UnityEngine;

public class ToCreditsTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StageManager.Instance.LoadMiscScene("Credits");
        }
    }
}
