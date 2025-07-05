using Unity.Cinemachine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class POIInteract : MonoBehaviour
{
    [Header("SCRIPTS")]
    public POIAttributes attributes;
    public PlanetPOIs PlanetPoints;
    public List<Locations> POIList;
    public POIobj currentPOI = null; // Stores the detected POI

    [Header("Settings")]
    public PlayerInput playerInput; // Handles input actions
    //private GameObject activePOIMenu = null; // Stores the spawned POI menu
    public Transform currentPlanet = null; // Stores the detected planet

    [Space(10)]
    public Transform DefaultCameraRotationObject;
    public Transform CameraRotationObject;

    [Header("POI")]
    public LayerMask POILayer; // LayerMask for "POIinteracts"
    public float POIinteractionRange = 50f; // How far the raycast detects POIs

    [Header("PLANET")]
    public LayerMask PlanetLayer; // LayerMask for planets
    public float PlanetInteractionRange = 500f;

    private CinemachineVirtualCamera activePlanetCam;

    [Header("UI")]
    public Transform menuParent; // The UI container for the menu
    public GameObject HoverUI;
    public GameObject StageSelectUI;
    public GameObject SelectOrLeave;

    private void Start()
    {
        CameraRotationObject = DefaultCameraRotationObject;
    }

    private void Update()
    {
        DetectPOI();
        DetectPlanet();

        if (currentPOI != null && playerInput.actions["Select"].WasPressedThisFrame())
        {
            Debug.Log("POI Selected: " + currentPOI.name);
            OpenPOIMenu();
        }

        if (currentPlanet != null && playerInput.actions["Select"].WasPressedThisFrame())
        {

            SwitchToPlanetCamera();
        }

        if (playerInput.actions["Back"].WasPressedThisFrame())
        {
            if (attributes.InSelectionMenu)
            {
                ClosePOIMenu();
            }
            else
            {
                ResetToCentreCamera();
            }
        }
    }

    void DetectPOI()
    {
        if (!attributes.InSelectionMenu && attributes.ZoomedOnPlanet)
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, POIinteractionRange, POILayer))
            {
                currentPOI = hit.transform.GetComponentInParent<POIobj>();
                currentPOI.DisplayHoverInfo();
            }
            else
            {
                currentPOI = null;
            }
        }
        else
        {
            currentPOI = null;
        }
    }

    void DetectPlanet()
    {
        if (!attributes.InSelectionMenu && !attributes.ZoomedOnPlanet)
        { 
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, PlanetInteractionRange, PlanetLayer))
            {
                currentPlanet = hit.transform;
                PlanetPoints = hit.transform.parent.GetComponent<PlanetPOIs>();
                POIList = PlanetPoints.LocationPOI;
                Debug.Log("Planet Hit: " + hit.transform.name); // Logs the Planet POI hit information
            }
            else
            {
                currentPlanet = null;
            }
        }
        else
        {
            currentPlanet = null;
        }
    }

    void SwitchToPlanetCamera()
    {
        if (currentPlanet == null) return;

        attributes.ZoomedOnPlanet = true;

        attributes.LastCam = attributes.CurrentCam; // Store the last active camera

        switch (currentPlanet.name)
        {
            case "EarthSelect":
                activePlanetCam = attributes.EarthCam;
                break;
            case "CarthageSelect":
                activePlanetCam = attributes.CarthageCam;
                break;
            case "MobiusSelect":
                activePlanetCam = attributes.MobiusCam;
                break;
            default:
                activePlanetCam = attributes.CentreUniCam;
                break;
        }

        if (activePlanetCam != null)
        {
            activePlanetCam.Priority = 20; // Make this camera active
            attributes.CurrentCam = activePlanetCam;
        }
        
        CameraRotationObject = PlanetPoints.ThisPlanetCameraRotator;
    }

    void ResetToCentreCamera()
    {
        if (attributes.CentreUniCam != null)
        {
            attributes.ZoomedOnPlanet = false;
            attributes.CentreUniCam.Priority = 20;
            if (attributes.CurrentCam != null)
            {
                attributes.CurrentCam.Priority = 10;
            }
            attributes.CurrentCam = attributes.CentreUniCam;
            CameraRotationObject = DefaultCameraRotationObject;
        }
    }

    void OpenPOIMenu()
    {
        if (currentPOI != null && currentPOI.poiData != null)
        {
            //// Destroy old menu if one exists
            //if (activePOIMenu != null)
            //{
            //    Destroy(activePOIMenu);
            //}

            //// Instantiate the new menu and set it as a child of menuParent
            //activePOIMenu = Instantiate(currentPOI.poiData.poiStageSelectMenu, menuParent);
            //activePOIMenu.transform.localPosition = Vector3.zero; // Reset position
            //activePOIMenu.transform.localScale = Vector3.one; // Reset scale

            HoverUI.SetActive(false);
            StageSelectUI.SetActive(true);
            attributes.InSelectionMenu = true;
            currentPOI.DisplayStageInfo();
        }
        else
        {
            Debug.LogWarning("No POI Data or POI Menu assigned.");
        }
    }

    void ClosePOIMenu()
    {
        if (attributes.InSelectionMenu)
        {
            StageSelectUI.SetActive(false);
            HoverUI.SetActive(true);
            attributes.InSelectionMenu = false;
        }
    }
}