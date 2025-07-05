using UnityEngine;

public class TardisButtonScript : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("TARDIS button activated!");
        // Add TARDIS functionality here
    }

    public string GetInteractionType()
    {
        return "Console Button";
    }
}
