using Luci;
using TMPro;
using UnityEngine;

public class DistFromPlayer : MonoBehaviour
{
    [SerializeField] bool giveToTMP;
    [SerializeField] TMP_Text text;
    Transform player;
    int distanceMetres = 0;

    private void OnEnable()
    {
        player = FindAnyObjectByType<FirstPersonController>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        distanceMetres = Mathf.RoundToInt(Vector3.Distance(player.position, transform.position));
        if (giveToTMP)
        {
            text.text = $"{distanceMetres}m";
        }
    }
}
