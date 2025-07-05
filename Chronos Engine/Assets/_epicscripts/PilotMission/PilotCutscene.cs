using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

// Begin with sercurity camera (payer cam) looking left and right,
// after 3 or so seconds the faux TARDIS material starts to become visible slowly, pusling as it goes to opaque
// when the material has 255 alpha, the proper tardis model is to be setactive(true)ed and the faux model is set false.
// tardis door(s) open and player has walking out animation as doors close behind them.
// camera priority goes to 1, so it wont be activated again.

public class PilotCutscene : MonoBehaviour
{
    public Material fauxTardisMaterial;
    public GameObject fauxTardis;
    public GameObject realTardis;

    public AudioSource SourceAudio;
    public AudioClip RematSound;

    public Animator Playeranim;
    public Animator Tardisanim;

    public CinemachineVirtualCamera cutsceneCam;
    public GameObject Player;

    [SerializeField] float rematDuration = 12f;
    [SerializeField] float rematThumpTime = 9.5f;
    [SerializeField] float cutsceneTimer = 0f;
    bool rematComplete = false;

    bool cutsceneBegun;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        Color currentCol = fauxTardisMaterial.GetColor("_BaseColor");
        currentCol.a = 0f;
        fauxTardisMaterial.SetColor("_BaseColor", currentCol);

        // Ensure material is in transparent mode
        fauxTardisMaterial.SetFloat("_Surface", 1); // Transparent
        fauxTardisMaterial.SetOverrideTag("RenderType", "Transparent");
        fauxTardisMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        fauxTardis.SetActive(true);
        realTardis.SetActive(false);

        SourceAudio.PlayOneShot(RematSound);
        cutsceneBegun = true;
    }

    void Update()
    {
        if (cutsceneBegun)
        {
            CutsceneStuff();
        }
    }

    void CutsceneStuff()
    {
        cutsceneTimer += Time.deltaTime;

        // Pulse alpha in a sine wave pattern to simulate flicker
        float progress = Mathf.Clamp01(cutsceneTimer / rematThumpTime);
        float flicker = Mathf.Abs(Mathf.Sin(cutsceneTimer * 1f)); // Flicker rate
        float targetAlpha = Mathf.Lerp(0f, 1f, progress) * flicker;

        if (cutsceneTimer >= rematThumpTime)
        {
            targetAlpha = 1f;
        }

        // faux tardis alpha
        Color currentCol = fauxTardisMaterial.GetColor("_BaseColor");
        currentCol.a = targetAlpha;
        fauxTardisMaterial.SetColor("_BaseColor", currentCol);

        // When remat is done
        if (cutsceneTimer >= rematDuration && !rematComplete)
        {
            rematComplete = true;

            // Swap faux for real TARDIS
            fauxTardis.SetActive(false);
            realTardis.SetActive(true);

            // Play TARDIS door open + player walk out anim
            Tardisanim.SetTrigger("OpenDoors"); // Make sure you have this trigger in your Animator
            Playeranim.SetTrigger("WalkOut");   // Same for this one

            // Optional: Set camera priority to 1 or switch cameras
            // (Handled elsewhere in your system presumably)
            cutsceneCam.Priority = 1;
            Player.SetActive(true);
        }
    }
}