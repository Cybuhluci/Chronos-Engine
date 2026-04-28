using UnityEngine;

public class ArmourManager : MonoBehaviour
{
    public ArmourSO[] currentArmour = new ArmourSO[2]; // index 0 is head, index 1 is torso. This can be expanded in the future (which we won't)
    public PlayerHealth playerHealth;

    public int overallDT => currentArmour != null ? CalculateOverallDT() : 0;

    private int CalculateOverallDT()
    {
        int totalDT = 0;
        foreach (var armour in currentArmour)
        {
            if (armour != null)
            {
                totalDT += armour._DT;
            }
        }
        return totalDT;
    }

    public int getOverallDT()
    {
        return overallDT;
    }

    public void ManageSelectedArmour(ArmourSO Armour)
    {
        if (Armour == null) return;
        if (Armour.armourType == ArmourSO.ArmourType.Head)
        {
            if (currentArmour[0] == Armour)
            {
                UnequipArmour(Armour);
            }
            else
            {
                EquipArmour(Armour);
            }
        }
        else if (Armour.armourType == ArmourSO.ArmourType.Torso)
        {
            if (currentArmour[1] == Armour)
            {
                UnequipArmour(Armour);
            }
            else
            {
                EquipArmour(Armour);
            }
        }
    }

    public void EquipArmour(ArmourSO newArmour)
    {
        if (newArmour == null) return;

        if (newArmour.armourType == ArmourSO.ArmourType.Head)
        {
            currentArmour[0] = newArmour;
        }
        else if (newArmour.armourType == ArmourSO.ArmourType.Torso)
        {
            currentArmour[1] = newArmour;
        }
    }

    public void UnequipArmour(ArmourSO SelectedArmour)
    {
        if (SelectedArmour.armourType == ArmourSO.ArmourType.Head)
        {
            currentArmour[0] = null;
        }
        else if (SelectedArmour.armourType == ArmourSO.ArmourType.Torso)
        {
            currentArmour[1] = null;
        }
    }
}