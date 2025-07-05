using TMPro;
using UnityEngine;

public class CollectCam : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public GameObject CollectibleCanvas;
    public TMP_Text ItemName;
    public GameObject collectInstant;
    public CollectItem ItemAttributes;

    private void Start()
    {
        CollectibleCanvas = GetComponentInParent<Canvas>().gameObject;
    }

    void Update()
    {
        ItemName.text = ItemAttributes.ItemName;

        float rotX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        float rotY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

        if (Input.GetMouseButton(0))
        {
            transform.Rotate(Vector3.up, -rotX, Space.World);
            transform.Rotate(Vector3.right, rotY, Space.World);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (CollectibleCanvas != null)
                CollectibleCanvas.SetActive(false);

            transform.rotation = Quaternion.identity;

            if (collectInstant != null)
                Destroy(collectInstant);

            PlayerPrefs.SetInt("CameraDisable", 0);
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
