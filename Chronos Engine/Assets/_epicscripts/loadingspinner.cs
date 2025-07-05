using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class loadingspinner : MonoBehaviour
{
    // Public variable for speed, adjustable in the Inspector
    public float spinSpeed = 100f;

    // Update is called once per frame
    void Update()
    {
        // Rotate the object around the Y-axis based on the spinSpeed
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f);
    }
}
