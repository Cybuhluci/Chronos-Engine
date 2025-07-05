using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class PlanetLocationCamera : MonoBehaviour
{
    [Header("SCRIPTS")]
    public POIAttributes attributes;
    public POIInteract interactor;
    [SerializeField] private List<Planet> Planets; // List of planets for selection
    // POIList from the interactor script, same as the planets one above, but with  locations on planets.

    [Header("Camera")]
    [SerializeField] private CinemachineBrain MainCamera;
    [SerializeField] private PlayerInput PlayerInput;
    [SerializeField] private float rotationSpeed = 5f; // Adjust speed for smoothness

    [Space(10)]
    [SerializeField] private Vector2 cameraInput;
    [SerializeField] private int currentPlanetIndex = 1;
    [SerializeField] private int currentLocationIndex = 1;
    [SerializeField] private Quaternion targetRotation; // Stores the target rotation

    [Header("POIs")]
    [SerializeField] private TMP_Text PlanetText;
    [SerializeField] private string PlanetName;
    [SerializeField] private Image PlanetImage;
    [SerializeField] private Sprite PlanetDefaultImage;
    [SerializeField] private int PlanetRaycastDistance;
    [SerializeField] private LayerMask PlanetCentreLayer;

    [Space(10)]
    [SerializeField] private Transform targetPOI = null;
    [SerializeField] private Planet selectedPlanet;
    [SerializeField] private Locations selectedLocation;
    [SerializeField] private Image SelectionBar;


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Invoke(nameof(NextPlanet), 0.69f); // Delay execution by 0.1 seconds
    }

    void NextPlanet()
    {
        SelectNextPlanet();
    }

    void Update()
    {
        if (attributes.InSelectionMenu) // Ignore input if in menu
        { }
        else
        {
            cameraInput = PlayerInput.actions["Camera"].ReadValue<Vector2>();

            if (attributes.ZoomedOnPlanet)
            {
                if (PlayerInput.actions["Camera"].WasPressedThisFrame()) // Does on planet POI
                {
                    if (cameraInput.x > 0.5f) SelectNextLocation();
                    else if (cameraInput.x < -0.5f) SelectPreviousLocation();

                    if (cameraInput.y > 0.5f) SelectNextLocation();
                    else if (cameraInput.y < -0.5f) SelectPreviousLocation();
                }
            }
            else if (!attributes.ZoomedOnPlanet)
            {
                if (PlayerInput.actions["Camera"].WasPressedThisFrame()) // Does POIs for the planets themselves
                {
                    if (cameraInput.x > 0.5f) SelectNextPlanet();
                    else if (cameraInput.x < -0.5f) SelectPreviousPlanet();

                    if (cameraInput.y > 0.5f) SelectNextPlanet();
                    else if (cameraInput.y < -0.5f) SelectPreviousPlanet();
                }
            }

            // Smoothly rotate towards the target rotation
            interactor.CameraRotationObject.rotation = Quaternion.Slerp(interactor.CameraRotationObject.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, PlanetRaycastDistance, PlanetCentreLayer))
        {
            PlanetName = selectedPlanet.PlanetName;
            PlanetText.text = selectedPlanet.PlanetName;
            PlanetImage.sprite = selectedPlanet.PlanetImage;
            Color NewColour;
            ColorUtility.TryParseHtmlString("#00c800", out NewColour);
            NewColour.a = 0.69f;
            SelectionBar.color = NewColour;
        }
        else
        {
            PlanetName = "Deep Space";
            PlanetText.text = PlanetName;
            PlanetImage.sprite = PlanetDefaultImage;
            Color NewColour;
            ColorUtility.TryParseHtmlString("#c80000", out NewColour);
            NewColour.a = 0.69f;
            SelectionBar.color = NewColour;
        }
    }

    void SelectNextPlanet()
    {
        if (Planets.Count == 0) return;
        currentPlanetIndex = (currentPlanetIndex + 1) % Planets.Count;
        UpdatePlanetSelection();
    }

    void SelectPreviousPlanet()
    {
        if (Planets.Count == 0) return;
        currentPlanetIndex = (currentPlanetIndex - 1 + Planets.Count) % Planets.Count;
        UpdatePlanetSelection();
    }

     void SelectNextLocation()
     {
        if (interactor.POIList.Count == 0) return;
        currentLocationIndex = (currentLocationIndex + 1) % interactor.POIList.Count;
        UpdateLocationSelection();
     }

    void SelectPreviousLocation()
    {
        if (interactor.POIList.Count == 0) return;
        currentLocationIndex = (currentLocationIndex - 1 + interactor.POIList.Count) % interactor.POIList.Count;
        UpdateLocationSelection();
    }

    void UpdatePlanetSelection()
    {
        if (Planets.Count > 0)
        {
            selectedPlanet = Planets[currentPlanetIndex];
            targetPOI = null; // Reset target POI
            targetRotation = selectedPlanet.PlanetTransform.rotation; // Set new target rotation
        }
    }

    void UpdateLocationSelection()
    {
        if (interactor.POIList.Count > 0)
        {
            selectedLocation = interactor.POIList[currentLocationIndex];
            targetPOI = null; // Reset target POI
            targetRotation = selectedLocation.LocationTransform.rotation; // Set new target rotation
        }
    }
}
