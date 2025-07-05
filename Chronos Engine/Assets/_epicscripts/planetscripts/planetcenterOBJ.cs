using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class planetcenterOBJ : MonoBehaviour
{
    public Transform CentreOBJ;
    public Vector3 ZeroPosition;

    private void Start()
    {
        ZeroPosition = CentreOBJ.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        CentreOBJ.position = ZeroPosition;  
    }
}
