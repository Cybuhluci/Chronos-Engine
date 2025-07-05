using Unity.Cinemachine;
using UnityEngine;

public class planetlookatcamera : MonoBehaviour
{
    public Transform cameraOBJ; // The center of the planet
    public Transform POIOBJ;    // The POI object that we want to rotate

    private void Start()
    {
        cameraOBJ = FindFirstObjectByType<CinemachineBrain>().transform;
    }

    // Start is called before the first frame update
    void Update()
    {
        // Make the POI object face away from the CentreOBJ
        Vector3 directionToCentre = cameraOBJ.position - POIOBJ.position; // Get the direction from POI to Centre
        Vector3 oppositeDirection = -directionToCentre; // Make it face away from the Centre

        // Calculate the rotation to look in the opposite direction
        Quaternion targetRotation = Quaternion.LookRotation(oppositeDirection);

        // Apply the rotation to the POI object
        POIOBJ.rotation = targetRotation;
    }
}
