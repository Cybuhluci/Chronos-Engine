using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image healthBar;

    private void Update()
    {
        healthBar.fillAmount = Mathf.Clamp01(playerHealth.CurrentHealth / playerHealth.MaxHealth);
    }
}
