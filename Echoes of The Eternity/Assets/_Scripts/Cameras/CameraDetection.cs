using UnityEngine;

public class CameraDetection : MonoBehaviour
{
    [SerializeField] bool isCameraWorking;

    [Header("Stealth View")]
    public float viewRadius = 12f;
    [Range(10f, 180f)] public float viewAngle = 90f;

    [Header("Suspicion Settings")]
    // Public is not changeable in-editor, this is because it should  be 0 nonetheless.
    private float susRatePublic = 0f;
    [SerializeField] private float susRatePrivate = 10f;
    [SerializeField] private float susRateSecure = 25f;
    [SerializeField] private float susDecayRate = 10f;

    // Update is called once per frame
    void Update()
    {
        if (!isCameraWorking) return;

        // make a cone from transform.forward with a radius of detectionDistance and an angle of FOV
        // if the player is in this cone, check their state (masked, casing) and what area they are in (public, private, secure)
        // then increase a suspicion meter based on these factors
    }

    public void GetShot()
    {
        isCameraWorking = false;
        Destroy(gameObject);
    }
}
