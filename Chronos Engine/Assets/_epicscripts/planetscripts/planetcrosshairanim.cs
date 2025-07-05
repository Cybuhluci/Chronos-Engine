using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class planetcrosshairanim : MonoBehaviour
{
    public Transform Crosshair1, Crosshair2;
    public float SpinSpeed1, SpinSpeed2;

    // Update is called once per frame
    void Update()
    {
        if (Crosshair1 != null)
        {
            Crosshair1.transform.Rotate(Vector3.forward, -SpinSpeed1 * Time.deltaTime);
        }
        if (Crosshair2 != null)
        {
            Crosshair2.transform.Rotate(Vector3.forward, -SpinSpeed2 * Time.deltaTime);
        }
    }
}
