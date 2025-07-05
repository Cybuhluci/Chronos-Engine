using Unity.Cinemachine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UI;

public class POIAttributes : MonoBehaviour
{
    public bool InSelectionMenu;

    public bool ZoomedOnPlanet;

    public string CurrentPlanet = "UniMap Centre";

    public CinemachineVirtualCamera CurrentCam;
    public CinemachineVirtualCamera LastCam;
    public CinemachineVirtualCamera CentreUniCam;
    public CinemachineVirtualCamera EarthCam;
    public CinemachineVirtualCamera CarthageCam;
    public CinemachineVirtualCamera MobiusCam;
}

[System.Serializable]
public class Planet
{
    public Transform PlanetTransform;
    public string PlanetName;
    public Sprite PlanetImage;

    public Planet(Transform Transform, string name, Sprite Image)
    {
        PlanetTransform = Transform;
        PlanetName = name;
        PlanetImage = Image;
    }
}

[System.Serializable]
public class Locations
{
    public Transform LocationTransform;
    public string LocationName;
    public Sprite Locationflag;
    public Sprite LocationScreenshot;
    [TextArea] public string LocationDescription;

    public Locations(Transform Transform, string name, Sprite Flag, Sprite Screen, string Description)
    {
        LocationTransform = Transform;
        LocationName = name;
        Locationflag = Flag;
        LocationScreenshot = Screen;
        LocationDescription = Description;
    }
}
