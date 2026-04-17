using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CollectiblePlanetCamera : MonoBehaviour
{
    //[Header("SCRIPTS")]
    //public POIAttributes attributes;
    //public POIInteract interactor;

    //[Header("Camera")]
    //public PlayerInput PlayerInput; // Handles input actions
    //public Transform CameraRotationObject; // The object that rotates the camera

    //public float rotationSpeed = 200f; // Speed of rotation
    //public float verticalClamp = 80f; // Limit how far up/down the camera can go
    //public float raycastRange; // How far the raycast detects POIs
    //public string TypeOfThingThatWasHit;

    //private Vector2 cameraInput; // Stores input from PlayerInput
    //private float currentVerticalRotation = 0f; // Tracks up/down rotation

    //[Header("POIs")]
    //public LayerMask POILayer; // LayerMask for POIs
    //public Transform targetPOI = null; // The POI the camera locks onto
    //public Quaternion targetPOIRotation;

    //[Header("Planets and Cameras")]
    //public CinemachineBrain MainCamera;
    //public LayerMask PlanetPOILayer;
    //public bool IsSelectingPlanet;
    //public float SelectionRaycastRange = 500f;
    //public float PlanetRaycastRange = 100f;

    //private void Start()
    //{
    //    Cursor.lockState = CursorLockMode.Locked;
    //}

    //void Update()
    //{
    //    if (!attributes.InSelectionMenu)
    //    {
    //        // Read input directly from PlayerInput
    //        cameraInput = PlayerInput.actions["Camera"].ReadValue<Vector2>();
    //    }
    //    else
    //    {
    //        cameraInput = Vector2.zero;
    //    }

    //    RotateCamera();
    //    DetectPOI();

    //    if (targetPOI != null)
    //    {
    //        targetPOIRotation = targetPOI.rotation;
    //    }
    //}

    //void RotateCamera()
    //{
    //    if (cameraInput != Vector2.zero)
    //    {
    //        float horizontal;
    //        float vertical;

    //        if (attributes.ZoomedOnPlanet)
    //        {
    //            horizontal = cameraInput.x * rotationSpeed * Time.deltaTime;
    //            vertical = cameraInput.y * rotationSpeed * Time.deltaTime;
    //        }
    //        else
    //        {
    //            horizontal = -cameraInput.x * rotationSpeed * Time.deltaTime;
    //            vertical = cameraInput.y * rotationSpeed * Time.deltaTime;
    //        }

    //        // Rotate left/right (Y-axis)
    //        CameraRotationObject.Rotate(Vector3.up, -horizontal, Space.World);

    //        // Clamp the up/down (X-axis) rotation
    //        currentVerticalRotation += -vertical;
    //        currentVerticalRotation = Mathf.Clamp(currentVerticalRotation, -verticalClamp, verticalClamp);

    //        CameraRotationObject.localRotation = Quaternion.Euler(currentVerticalRotation, CameraRotationObject.localRotation.eulerAngles.y, 0);
    //    }
    //    else if (targetPOI != null)
    //    {
    //        // If no input is detected, smoothly rotate towards the target POI while maintaining the current Y-axis rotation
    //        RotateToPOI();
    //    }
    //    else
    //    {
    //        // Ensure the camera smoothly transitions even if no input is detected, and we aren't locked to a POI
    //        SmoothCameraRotation();
    //    }
    //}

    //void SmoothCameraRotation()
    //{
    //    // Add a fallback smooth camera transition to avoid the crosshair going desynced
    //    // This smoothly rotates the camera back to the POI or default position
    //    if (targetPOI != null)
    //    {
    //        Quaternion targetRotation = Quaternion.LookRotation(targetPOI.position - CameraRotationObject.position);
    //        CameraRotationObject.rotation = Quaternion.Slerp(CameraRotationObject.rotation, targetRotation, Time.deltaTime * 2f);
    //    }
    //}

    //void DetectPOI()
    //{
    //    // Raycast for POIs
    //    Ray poiRay = new Ray(MainCamera.transform.position, MainCamera.transform.forward);
    //    RaycastHit poiHit;

    //    if (Physics.Raycast(poiRay, out poiHit, PlanetRaycastRange, POILayer))
    //    {
    //        Debug.Log("POI Hit: " + poiHit.transform.name); // Logs the POI hit information
    //        if (targetPOI == null)
    //        {
    //            targetPOI = poiHit.transform; // Lock onto the POI
    //            TypeOfThingThatWasHit = "POI";
    //        }
    //    }
    //    else
    //    {
    //        targetPOI = null; // Reset if no POI is detected
    //    }

    //    if (!attributes.ZoomedOnPlanet)
    //    {
    //        // Raycast for Planet POIs
    //        Ray planetRay = new Ray(MainCamera.transform.position, MainCamera.transform.forward);
    //        RaycastHit planetHit;

    //        if (Physics.Raycast(planetRay, out planetHit, SelectionRaycastRange, PlanetPOILayer))
    //        {
    //            targetPOI = planetHit.transform;
    //            TypeOfThingThatWasHit = "Planet";
    //            Debug.Log("Planet POI Hit: " + planetHit.transform.name); // Logs the Planet POI hit information
    //        }
    //    }
    //}

    //void RotateToPOI()
    //{
    //    if (targetPOI == null) return;

    //    // Get the desired rotation based on the target POI
    //    Quaternion targetRotation = Quaternion.LookRotation(targetPOI.position - CameraRotationObject.position);

    //    // Smoothly rotate towards POI without worrying about Euler angle wrapping
    //    CameraRotationObject.rotation = Quaternion.Slerp(CameraRotationObject.rotation, targetRotation, Time.deltaTime * 2f);
    //}
}
