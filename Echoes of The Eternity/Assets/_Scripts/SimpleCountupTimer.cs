using TMPro;
using UnityEngine;

public class SimpleCountupTimer : MonoBehaviour
{
    [SerializeField] private float countupTime = 0f;
    [SerializeField] private TMP_Text timerText;

    // Update is called once per frame
    void Update()
    {
        countupTime += Time.deltaTime;
        if (timerText != null)
        {
            timerText.text = countupTime.ToString("F0");
        }
    }
}
