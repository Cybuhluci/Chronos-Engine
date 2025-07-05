using System.Collections.Generic;
using UnityEngine;

public class missionLIST : MonoBehaviour
{
    public List<Mission> missions = new List<Mission>();

    public void PopulateMissions()
    {
        // MoonBase
        missions.Add(new Mission("Moonbase Charlie", "Escape before the Vashta Nerada consume everything.",
            new List<string> { "Find Commander Ayla", "Restore power to the escape pods", "Escape the Moonbase" }));

        // TARDIS filler comedy
        missions.Add(new Mission("TARDIS Trouble", "Something’s wrong with the chameleon circuit again.",
            new List<string> { "Find the malfunction", "Rewire the control panel", "Fix the cloaking system" }));

        // Dalek City
        missions.Add(new Mission("The Dalek World", "Assist the resistance in their final stand.",
            new List<string> { "Locate the resistance base", "Defend against Dalek assault", "Destroy the Dalek command hub" }));

        // Planet XI
        missions.Add(new Mission("Planet XI", "Investigate the mysterious signal.",
            new List<string> { "Find the origin of the signal", "Decipher the alien message", "Survive the ambush" }));

        // Frozen Academy
        missions.Add(new Mission("Frozen Academy", "Uncover the secrets of the abandoned school.",
            new List<string> { "Search for clues", "Escape the ice creatures", "Unlock the headmaster’s archives" }));

        // TARDIS filler 2: Electric Boogaloo
        missions.Add(new Mission("TARDIS Mayhem", "The TARDIS doors won’t close… again.",
            new List<string> { "Identify the interference", "Reconfigure the dimension stabilizers", "Seal the temporal breach" }));

        // Dying Planet
        missions.Add(new Mission("Dying Planet", "Find a way to restore the atmosphere before it's too late.",
            new List<string> { "Activate the planetary shield", "Repair the oxygen generators", "Evacuate the survivors" }));

        // Warped Colony
        missions.Add(new Mission("Warped Colony", "The settlers are experiencing impossible events.",
            new List<string> { "Investigate the anomalies", "Uncover the cause", "Escape before reality collapses" }));

        // Ancient Gallifreyan Tunnels
        missions.Add(new Mission("The Time Vaults", "Explore the ruins of ancient Gallifrey.",
            new List<string> { "Decode the inscriptions", "Avoid the time traps", "Discover the hidden relic" }));

        // Bounty Hunters Hunt
        missions.Add(new Mission("Bounty Hunters’ Hunt", "You’re the target—stay ahead of the hunters.",
            new List<string> { "Find a safehouse", "Gather intelligence", "Turn the tables on your pursuers" }));
    }
}
