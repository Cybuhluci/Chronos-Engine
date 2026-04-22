using UnityEngine;
using Luci;

public class ViewmodelBob : MonoBehaviour
{
    public enum BobVersion
    {
        Source, // no bobbing at all, weapon will be perfectly still in the player's view - like Half-Life 2.
        Arctic, // a more modern bobbing style with subtle movements and rotations.
        Xiland // a stylized bobbing style inspired by the B42 Fallout New Vegas mod series & Hit's Locomotion mod, with distinct full-range movements.
    }
    public BobVersion bobVersion = BobVersion.Source;

    public Transform weaponParentTransform; // the thing that actually bobs.

    [Header("Source Bob Settings")]
    public float sourceBaseSpeed = 7f; // base frequency of bob when moving
    public Vector2 sourceBobAmount = new Vector2(0.06f, 0.04f); // x(horizontal), y(vertical)
    public float sourceSpeedNormalization = 5f; // speed that maps to 1.0 bob intensity
    public float sourceLerpSpeed = 8f; // how quickly we lerp to bob target

    [Header("References")]
    public FirstPersonController playerController; // optional - used to sample player velocity

    // internals
    private Vector3 _originalLocalPos;
    private Quaternion _originalLocalRot;
    private float _bobTimer = 0f;

    private Transform _bobTarget;
    private Vector3 _currentBobOffset = Vector3.zero;

    private void Awake()
    {
        if (weaponParentTransform == null)
            weaponParentTransform = transform;

        _originalLocalPos = weaponParentTransform.localPosition;
        _originalLocalRot = weaponParentTransform.localRotation;
        _bobTarget = weaponParentTransform;
    }

    private void OnValidate()
    {
        if (weaponParentTransform == null)
            weaponParentTransform = transform;
    }

    private void Update()
    {
        switch (bobVersion)
        {
            case BobVersion.Source:
                ApplySourceBob();
                break;
            case BobVersion.Arctic:
                ApplyArcticBob();
                break;
            case BobVersion.Xiland:
                ApplyXilandBob();
                break;
            default:
                ApplySourceBob();
                break;
        }
    }

    private void ApplySourceBob()
    {
        // "Source" option: no bobbing. Smoothly return to original transform.
        weaponParentTransform.localPosition = Vector3.Lerp(weaponParentTransform.localPosition, _originalLocalPos, Time.deltaTime * sourceLerpSpeed);
        weaponParentTransform.localRotation = Quaternion.Slerp(weaponParentTransform.localRotation, _originalLocalRot, Time.deltaTime * sourceLerpSpeed);
        _bobTimer = 0f;
        _currentBobOffset = Vector3.zero;
    }

    private void ApplyArcticBob()
    // how i think this should work:
    // this bob should be the type of one you see in more modern games, like cod.
    // It should be a more dynamic bob that changes based on player speed,
    // with more exaggerated movements at higher speeds, and more subtle movements at lower speeds.
    // It should also have a bit of procedural randomness to make it feel less repetitive,
    // and should include some rotational movement for extra feel.
    {
        // Modern "arctic" bob: layered bobs + subtle noise + rotation
        float speed = 0f;
        if (playerController != null)
        {
            var vel = playerController.playerVelocity;
            Vector3 horiz = new Vector3(vel.x, 0f, vel.z);
            speed = horiz.magnitude;
        }

        float intensity = Mathf.Clamp01(speed / Mathf.Max(0.0001f, sourceSpeedNormalization));

        // primary bob (low frequency)
        _bobTimer += Time.deltaTime * (sourceBaseSpeed * (0.6f + intensity));
        float primaryX = Mathf.Sin(_bobTimer) * sourceBobAmount.x * Mathf.Lerp(0.1f, 1f, intensity);
        float primaryY = Mathf.Sin(_bobTimer * 2f) * sourceBobAmount.y * 0.6f * Mathf.Lerp(0.1f, 1f, intensity);

        // secondary gentle sway (higher frequency, lower amplitude)
        float sway = Mathf.Sin(_bobTimer * 1.9f) * sourceBobAmount.x * 0.25f * Mathf.Lerp(0f, 1f, intensity);

        // add subtle perlin noise so animation isn't perfectly looping
        float noise = (Mathf.PerlinNoise(Time.time * 0.5f + transform.GetInstanceID(), 0f) - 0.5f) * 0.02f;

        Vector3 targetOffset = new Vector3(primaryX + sway + noise, primaryY + Mathf.Abs(noise) * 0.5f, 0f);
        _currentBobOffset = Vector3.Lerp(_currentBobOffset, targetOffset, Time.deltaTime * (sourceLerpSpeed * 0.9f + intensity * 6f));

        // apply position
        weaponParentTransform.localPosition = _originalLocalPos + _currentBobOffset;

        // rotation: more subtle but responsive; tilt on X/pitch and Y/yaw a little
        float rotX = Mathf.Lerp(0f, _currentBobOffset.y * 35f, 0.8f);
        float rotY = Mathf.Lerp(0f, _currentBobOffset.x * -20f, 0.6f);
        float rotZ = Mathf.Sin(_bobTimer * 0.5f) * 0.5f * intensity;
        Quaternion targetRot = Quaternion.Euler(rotX, rotY, rotZ);
        weaponParentTransform.localRotation = Quaternion.Slerp(weaponParentTransform.localRotation, _originalLocalRot * targetRot, Time.deltaTime * (sourceLerpSpeed * 0.8f + intensity * 4f));
    }

    private void ApplyXilandBob()
    // how i think this bob should work:
    // this bob should be the type of one you see in more realistic games or mods, like the B42 FNV mod or Hit's Locomotion mod,
    // where the weapon has distinct full-range movements that are more exaggerated than the typical sine wave bob,
    // but still feel grounded and responsive to player movement. It should also have a bit of procedural randomness to make it feel less repetitive.
    {
        // placeholder, just use arctic bob for now
        ApplyArcticBob();
    }

    // unlike inertia, this is a script to make the weapon "bob" up and down, left and right as if someone was actually holding it.
    // the complex part is really just making it look nice, as well as making it player-velocity based,
    // and state based (i.e. walking, running, crouching, jumping/falling, etc.)
}

