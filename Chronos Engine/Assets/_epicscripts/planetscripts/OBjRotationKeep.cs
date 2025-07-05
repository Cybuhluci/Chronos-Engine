using UnityEngine;

public class OBjRotationKeep : MonoBehaviour
{
    public Transform OBJ;
    public Quaternion OGrotation;

    void Start()
    {
        OGrotation = OBJ.rotation;
    }

    void Update()
    {
        OBJ.rotation = OGrotation;
    }
}
