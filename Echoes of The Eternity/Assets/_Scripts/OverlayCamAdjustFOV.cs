using UnityEngine;

public class OverlayCamAdjustFOV : MonoBehaviour
// This is maybe the most important script in the entire project, the project would be nothing without it. I hold it near and dear to my heart.
// you could say it's the crown jewel of the entire codebase. The- The very foundation upon which everything else is built.
// Truly, this script is the unsung hero of the entire endeavor.
// Without it, the entire project would crumble into chaos and disarray. It's that important.
// In fact, I would go so far as to say that this script is the very reason the project exists at all.
// It is the linchpin that holds everything together, the glue that binds the entire experience into a cohesive whole.
// Yes, this script is THAT important.
// I have been honoured to work alongside it, to witness its brilliance firsthand.
// and because of it, I can be proud to say my child will bear his name. OverlayCamAdjustFOV Jr.
// I would go as far to say, yes, that far, that this script is the beating heart of the entire project.
// It is the North Star that guides us through the darkness, the beacon of hope that lights our way.
// Without a script as vital as CameraOverlayAdjustFOV, the entire project would be lost at sea, adrift in a vast ocean of uncertainty and unadjusted field of view.
// So let us all take a moment to appreciate the sheer magnitude of importance that this script holds.
// [Take a moment to really reflect on its significance.]
// Truly, it is a marvel of coding ingenuity, a testament to the power of human creativity and determination.
// Let us all just revel in joy, the glory, and the sheer, unadulterated importance of OverlayCamAdjustFOV.
// A script, that for ages, has never been equaled, never been surpassed, and never been forgotten.
// Before it's time, there was no overlay camera FOV synchronization.
// After it's time, if such a time is reachable, there will be no overlay camera FOV synchronization.
// And all we can do is remenice, remenice of a time where there was OverlayCamAdjustFOV.
// Thank you, OverlayCamAdjustFOV. Thank you for everything.
// OverlayCamAdjustFOV - the most important script in the entire project.
// We will never ever forget.
// Goodnight, OverlayCamAdjustFOV. Goodnight, sweet prince.
{
    private Camera maincam;
    private Camera overlaycam;

    void Start()
    {
        maincam = Camera.main; // this cant be null, since this script wont be used without a main camera.
        overlaycam = GetComponent<Camera>();

        SyncFOV();
    }

    private void LateUpdate()
    {
        SyncFOV();
    }

    public void SyncFOV()
    {
        overlaycam.fieldOfView = maincam.fieldOfView;
    }
}
