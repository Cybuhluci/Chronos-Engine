using UnityEngine;

public class QuestMarkerLTMC : MonoBehaviour
{
    [Tooltip("If true the object will face away from the main camera (useful for world-space canvases). If false it will LookAt the camera.")]
    public bool faceAwayFromCamera = false;
    [Tooltip("Desired size of the marker on screen (as a fraction of screen height).")]
    public float desiredScreenSize = 0.5f;

    void LateUpdate()
    {
        var cam = Camera.main;
        if (cam == null) return;
        // Billboard behaviour: make the marker face parallel to the camera (so its face is aligned with camera view)
        // If faceAwayFromCamera is true the marker's forward will match the camera forward (i.e. "face away" from camera position),
        // otherwise it will face toward the camera (inverse forward).
        Vector3 camForward = cam.transform.forward;
        Vector3 forward = faceAwayFromCamera ? camForward : -camForward;
        // Keep marker upright in world space
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

        // Keep marker approximately constant size on screen by scaling with distance.
        // Tweak the scaleMultiplier to get desired on-screen size.
        float distance = Vector3.Distance(transform.position, cam.transform.position);
        float scaleMultiplier = desiredScreenSize; // smaller value -> smaller on-screen size; adjust as needed
        float scaleFactor = Mathf.Max(0.0001f, distance * scaleMultiplier);
        transform.localScale = Vector3.one * scaleFactor;
    }
}
