using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanetNameBar : MonoBehaviour
{
    [Header("Attributes")]
    public POIAttributes attributes;

    [Header("UI Elements")]
    public GameObject timeVortexObject; // The Time Vortex GameObject to activate/deactivate
    public List<PlanetImageMapping> planetImages; // List of planet names and their respective image objects

    private GameObject activePlanetImage; // Currently active planet name image

    private void Start()
    {
        UpdatePlanetName();
    }

    void FixedUpdate()
    {
        UpdatePlanetName();
    }

    void UpdatePlanetName()
    {
        SetActivePlanetImage(GetPlanetImage(attributes.CurrentPlanet)); // Show correct planet name image
    }

    private GameObject GetPlanetImage(string planetName)
    {
        foreach (var mapping in planetImages)
        {
            if (mapping.planetName == planetName)
            {
                return mapping.planetImage;
            }
        }
        return null; // No matching planet image found
    }

    private void SetActivePlanetImage(GameObject newImage)
    {
        if (activePlanetImage != null)
        {
            activePlanetImage.SetActive(false); // Hide previous image
        }

        activePlanetImage = newImage;

        if (activePlanetImage != null)
        {
            activePlanetImage.SetActive(true); // Show new image
        }
    }
}

[System.Serializable]
public class PlanetImageMapping
{
    public string planetName;
    public GameObject planetImage; // The actual GameObject for this planet's name image
}
