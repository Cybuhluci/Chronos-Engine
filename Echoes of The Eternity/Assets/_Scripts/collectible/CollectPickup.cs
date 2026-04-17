using Luci.Interactions;
using Luci.Saving;
using UnityEngine;

public class CollectPickup : MonoBehaviour, IInteractable
{
    public CollectItem Item;
    public CollectCam collectycam;
    public SaveManager saveManager;

    [Space(5)]
    public int CollectibleCameraLayer = 16;
    public GameObject CollectibleCanvas;
    public GameObject CollectibleEmpty;
    public GameObject CollectibleModel;
    public GameObject collectInstant;

    private void Start()
    {
        //if (saveManager.HasCollected(Item.CollectibleCode))
        //    gameObject.SetActive(false);
    }

    public void PressInteract()
    {
        //saveManager.Collect(Item);

        PlayerPrefs.SetInt("CameraDisable", 1);
        CollectibleCanvas.SetActive(true);
        //CollectibleModel.layer = CollectibleCameraLayer;
        collectInstant = Instantiate(CollectibleModel, CollectibleEmpty.transform);
        Cursor.lockState = CursorLockMode.Confined;
        collectycam.collectInstant = collectInstant;
        collectycam.ItemAttributes = Item;
        SetLayerRecursively(collectInstant, CollectibleCameraLayer);
        gameObject.SetActive(false);
    }

    public InteractionType GetInteractionType()
    {
        return InteractionType.Collect;
    }

    public string GetInteractionPrompt()
    {
        return $"{Item.ItemName}";
    }

    public void OnInteract(GameObject interactor)
    {
        PressInteract();
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
