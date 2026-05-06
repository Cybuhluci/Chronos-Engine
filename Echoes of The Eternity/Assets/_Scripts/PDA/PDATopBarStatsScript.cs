using TMPro;
using UnityEngine;

public class PDATopBarStatsScript : MonoBehaviour
{
    public PlayerHealth playerHealth;
    [SerializeField] TMP_Text healthText;
    public ArmourManager armourManager;
    [SerializeField] TMP_Text _DTText;
    [SerializeField] TMP_Text _DRText;
    public InventoryManager inventoryManager;
    [SerializeField] TMP_Text QuidText;

    // Update is called once per frame
    void Update()
    {
        healthText.text = "HP: " + playerHealth.CurrentHealth;
        _DTText.text = "DT: " + armourManager.getOverallDT();
        _DRText.text = "DR: " + armourManager.getOverallDR();
        QuidText.text = "Quid: 0";    
    }
}
