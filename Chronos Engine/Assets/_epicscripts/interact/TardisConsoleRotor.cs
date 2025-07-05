using UnityEngine;

public class TardisConsoleRotor : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 20f; // How fast it spins

    [Header("Vertical Movement Settings")]
    public float verticalRange = 0.5f; // How far it moves up/down
    public float verticalSpeed = 1f; // How fast it moves up/down

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position; // Save initial position
    }

    void Update()
    {
        // Spin the rotor
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.Self);

        // Move up and down smoothly
        float newY = startPosition.y + Mathf.Sin(Time.time * verticalSpeed) * verticalRange;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
}
