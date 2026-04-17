using UnityEngine;

public class ViewmodelBob : MonoBehaviour
{
    private enum BobVersion
    {
        None, // no bobbing at all, weapon will be perfectly still in the player's view
        Source, // the classic "Source engine" style bob, which is a simple sine wave based on player velocity.
                // It's pretty basic, but it gets the job done and is very performant.
        Arctic, // a more modern bobbing style with subtle movements and rotations.
        Xiland // a stylized bobbing style inspired by the B42 Fallout New Vegas mod series & Hit's Locomotion mod, with distinct full-range movements.
    }

    // unlike inertia, this is a script to make the weapon "bob" up and down, left and right as if someone was actually holding it.
    // the complex part is really just making it look nice, as well as making it player-velocity based, and state based (i.e. walking, running, crouching, jumping/falling, etc.)
}
