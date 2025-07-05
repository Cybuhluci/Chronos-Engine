using UnityEngine;

public class CameraMove : MonoBehaviour
{
    // i eat variables.
    // wait, whats this below me?
    public Transform orientation;      
    public GameObject cameraAlignmentBeam;
    public LayerMask groundLayer;  
    public float mouseSensitivity = 100f; 
    public float distanceFromPlayer = 5f; 

    private float mouseX, mouseY;    
    private float xRotation = 0f; 
    private Vector3 surfaceNormal = Vector3.up;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Locks the cursor so you dont click off
    }

    private void Update()
    {
        // running functions per frame 
        HandleMouseInput();
        AlignToSurface();
        AdjustCameraPosition();
    }

    private void HandleMouseInput()
    {
        // mouse inputs
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Clamp vertical movement to stop camera doing flips
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -45f, 85f);

        // Rotate orientation horizontally
        orientation.localRotation = Quaternion.Euler(xRotation, orientation.eulerAngles.y + mouseX, 0f);
    }

    private void AlignToSurface()
    {
        RaycastHit hit; // the raycast we will use for the alignment (he prefers the name Jim)

        // raycast from the alignment beam to detect the ground below orientation
        if (Physics.Raycast(cameraAlignmentBeam.transform.position, -cameraAlignmentBeam.transform.up, out hit, distanceFromPlayer, groundLayer))
        {
            surfaceNormal = hit.normal; // make the surface normal the normal of the surface hit
        }
        else
        {
            surfaceNormal = Vector3.up; // make surface normal world up so camera goes back to normal
        }

        // align camera with the surface normal
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, surfaceNormal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        // i can code well when i want, this was one of those times
    }

    private void AdjustCameraPosition()
    {
        // get local up for the orientation
        Vector3 localUp = orientation.up;

        // offsets camera so the camera looks nice
        Vector3 targetPosition = orientation.position + localUp - orientation.forward * distanceFromPlayer;

        // makes the camera movement smooth because it i think its nice
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);

        // forces camera to look at orientation, sonic must stay in the middle of the screen
        transform.LookAt(orientation.position + localUp);
    }
}
