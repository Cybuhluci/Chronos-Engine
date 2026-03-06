using UnityEngine;
using UnityEngine.UI;

public abstract class UniqueDeployableMainScript : MonoBehaviour
{
    public abstract void ToggleUniqueDeployable();
    
    // Refill the UD meter by percentage (0-100). Concrete implementations should apply the refill
    // and add charges if the meter completes. Percentage is expected to be a 0-100 value.
    
    // Refill the UD meter by percentage (0-100). Concrete implementations should apply the refill
    // and add charges if the meter completes. Percentage is expected to be a 0-100 value.
    public abstract void RefillUDMeter(float percentage);
}
