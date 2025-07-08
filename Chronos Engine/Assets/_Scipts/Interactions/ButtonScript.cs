using UnityEngine;
using UnityEngine.Events;

public class ButtonScript : MonoBehaviour, IInteractable
{
    [Header("Button Events")]
    public UnityEvent onButtonPressed;  

    public void Interact()
    {
        Debug.Log("Button pressed!");
        onButtonPressed.Invoke();  // Calls whatever methods are assigned in the Inspector
    }

    public EInteractionType GetInteractionType()
    {
        return EInteractionType.InteractShort;
    }
}