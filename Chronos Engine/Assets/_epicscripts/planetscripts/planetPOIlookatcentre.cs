using UnityEngine;

public class PlanetPOILookAtCentre : MonoBehaviour
{
    public Transform CentreOBJ; // The center of the planet
    public Transform POIOBJ;    // The POI object that we want to rotate

    public bool LookAt = true; // If true, face towards the centre; if false, face away

    void Update()
    {
        if (CentreOBJ == null || POIOBJ == null) return; // Ensure objects are assigned

        // Get the direction to the centre
        Vector3 directionToCentre = (CentreOBJ.position - POIOBJ.position).normalized;
        Vector3 targetDirection = LookAt ? directionToCentre : -directionToCentre;

        // Apply rotation using quaternions
        POIOBJ.rotation = Quaternion.LookRotation(targetDirection);
    }
}
