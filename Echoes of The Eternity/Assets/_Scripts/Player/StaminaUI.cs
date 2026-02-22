using StarterAssets;
using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    public FirstPersonController personcontrol;
    public Slider staminaSlider;

    private void Start()
    {
        staminaSlider.maxValue = personcontrol.MaxStamina;
    }

    void Update()
    {
        float stamina = personcontrol.Stamina;
        staminaSlider.value = stamina;
    }
}
