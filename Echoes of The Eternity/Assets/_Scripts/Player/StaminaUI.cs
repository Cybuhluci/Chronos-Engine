using Luci;
using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    public FirstPersonController personcontrol;
    public Image staminaSlider;

    private void Update()
    {
        staminaSlider.fillAmount = Mathf.Clamp01(personcontrol.Stamina / personcontrol.MaxStamina);
    }
}
