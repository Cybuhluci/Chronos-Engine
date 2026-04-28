using UnityEngine;
using UnityEngine.SceneManagement;

public class CommandGenerator : MonoBehaviour
{
    [SerializeField] private ConsoleManager consoleManager; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CommandRegistry.Register(new ConsoleCommand
        {
            Name = "echo",
            Description = "Echoes the input.",
            Execute = args =>
            {
                string msg = string.Join(" ", args);
                Debug.Log(msg);
                consoleManager.AppendOutput(msg);
            }
        });

        // AI disable/enable: AI_Disable 0|1
        CommandRegistry.Register(new ConsoleCommand
        {
            Name = "AI_Disable",
            Description = "Disable or enable all enemy AI. Usage: AI_Disable 0|1",
            Execute = args =>
            {
                if (PlayerPrefs.GetInt("sv_cheats", 0) == 0)
                {
                    string err = "Cheats are disabled.";
                    Debug.LogWarning(err);
                    consoleManager.AppendOutput(err);
                    return;
                }
                bool enable = true;
                if (args.Length > 0 && (args[0] == "0")) enable = false;
                var enemies = GameObject.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
                int count = 0;
                foreach (var e in enemies)
                {
                    if (e == null) continue;
                    e.enabled = enable;
                    var nav = e.GetComponent<UnityEngine.AI.NavMeshAgent>();
                    if (nav != null) nav.isStopped = !enable;
                    count++;
                }
                string msg = $"AI {(enable ? "enabled" : "disabled")} for {count} enemies";
                Debug.Log(msg);
                consoleManager.AppendOutput(msg);
            }
        });

        // SpawnEnemy: tries Resources/Enemies/<name> or uses EnemyButtonSpawner if no arg
        CommandRegistry.Register(new ConsoleCommand
        {
            Name = "SpawnEnemy",
            Description = "Spawns an enemy. Usage: SpawnEnemy <prefabName> (looks in Resources/Enemies) or no args to use spawners",
            Execute = args =>
            {
                if (args.Length > 0)
                {
                    if (PlayerPrefs.GetInt("sv_cheats", 0) == 0)
                    {
                        string err = "Cheats are disabled.";
                        Debug.LogWarning(err);
                        consoleManager.AppendOutput(err);
                        return;
                    }
                    string prefabName = args[0];
                    var prefab = Resources.Load<GameObject>("Enemies/" + prefabName);
                    if (prefab != null)
                    {
                        var player = GameObject.FindGameObjectWithTag("Player");
                        Vector3 pos = player != null ? player.transform.position + player.transform.forward * 2f : Vector3.zero;
                        GameObject.Instantiate(prefab, pos, Quaternion.identity);
                        string msg = $"Spawned enemy prefab '{prefabName}'";
                        Debug.Log(msg);
                        consoleManager.AppendOutput(msg);
                        return;
                    }
                    else
                    {
                        string err = $"Prefab 'Enemies/{prefabName}' not found.";
                        Debug.LogWarning(err);
                        consoleManager.AppendOutput(err);
                        return;
                    }
                }
            }
        });

        // God: refill health and armour
        CommandRegistry.Register(new ConsoleCommand
        {
            Name = "God",
            Description = "Make the player unable to die.",
            Execute = args =>
            {
                if (PlayerPrefs.GetInt("sv_cheats", 0) == 0)
                {
                    string err = "Cheats are disabled.";
                    Debug.LogWarning(err);
                    consoleManager.AppendOutput(err);
                    return;
                }
                var ph = GameObject.FindFirstObjectByType<PlayerHealth>();
                if (ph != null)
                {
                    ph._isInvulnerable = !ph._isInvulnerable;
                    string err = $"godmode {ph._isInvulnerable}.";
                    Debug.LogWarning(err);
                    consoleManager.AppendOutput(err);
                }
                else
                {
                    string err = "PlayerHealth not found.";
                    Debug.LogWarning(err);
                    consoleManager.AppendOutput(err);
                }
            }
        });

        // noclip: toggle CharacterController on player
        CommandRegistry.Register(new ConsoleCommand
        {
            Name = "noclip",
            Description = "Toggle noclip (disables CharacterController).",
            Execute = args =>
            {
                if (PlayerPrefs.GetInt("sv_cheats", 0) == 0)
                {
                    string err = "Cheats are disabled.";
                    Debug.LogWarning(err);
                    consoleManager.AppendOutput(err);
                    return;
                }
                var player = GameObject.FindFirstObjectByType<Luci.FirstPersonController>();
                if (player == null)
                {
                    consoleManager.AppendOutput("Player object not found.");
                    return;
                }
                player.ToggleNoclip(!player.noclipEnabled);
                string msg = $"Noclip {(player.noclipEnabled ? "off" : "on")}";
                Debug.Log(msg);
                consoleManager.AppendOutput(msg);
            }
        });

        // kill: apply large damage to player
        CommandRegistry.Register(new ConsoleCommand
        {
            Name = "kill",
            Description = "Kills the player instantly.",
            Execute = args =>
            {
                var ph = GameObject.FindFirstObjectByType<PlayerHealth>();
                if (ph != null)
                {
                    ph.TakeDamage(99999f);
                    string msg = "Player downed.";
                    Debug.Log(msg);
                    consoleManager.AppendOutput(msg);
                }
                else
                {
                    string err = "PlayerHealth not found.";
                    Debug.LogWarning(err);
                    consoleManager.AppendOutput(err);
                }
            }
        });

        // quit -> exit application
        CommandRegistry.Register(new ConsoleCommand
        {
            Name = "quit",
            Description = "Quit the application.",
            Execute = args =>
            {
                consoleManager.AppendOutput("Quitting application...");
                Application.Quit();
# if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
# endif
            }
        });

        // exit -> return to main menu
        CommandRegistry.Register(new ConsoleCommand
        {
            Name = "exit",
            Description = "Exit to main menu.",
            Execute = args =>
            {
                consoleManager.AppendOutput("Returning to main menu...");
                if (StageManager.Instance != null) StageManager.Instance.LoadMiscScene("mainmenu");
                else UnityEngine.SceneManagement.SceneManager.LoadScene("mainmenu");
            }
        });

        // sv_cheats 0/1 -> set PlayerPrefs flag
        CommandRegistry.Register(new ConsoleCommand
        {
            Name = "sv_cheats",
            Description = "Toggle server cheats (sv_cheats 0|1)",
            Execute = args =>
            {
                if (args.Length == 0)
                {
                    consoleManager.AppendOutput("Usage: sv_cheats 0|1");
                    return;
                }
                int val = (args[0] == "1") ? 1 : 0;
                PlayerPrefs.SetInt("sv_cheats", val);
                PlayerPrefs.Save();
                consoleManager.AppendOutput($"sv_cheats set to {val}");
            }
        });

        // giveammo -> refill all GunController ammo
        CommandRegistry.Register(new ConsoleCommand
        {
            Name = "giveammo",
            Description = "Give the player full ammo for current weapons.",
            Execute = args =>
            {
                string msg = $"This function is changing soon, sorry.";
                Debug.Log(msg);
                consoleManager.AppendOutput(msg);
            }
        });

        // givehealth -> refill player's health and armour
        CommandRegistry.Register(new ConsoleCommand
        {
            Name = "givehealth",
            Description = "Refill player's health and armour.",
            Execute = args =>
            {
                if (PlayerPrefs.GetInt("sv_cheats", 0) == 0)
                {
                    string err = "Cheats are disabled.";
                    Debug.LogWarning(err);
                    consoleManager.AppendOutput(err);
                    return;
                }
                var ph = GameObject.FindFirstObjectByType<PlayerHealth>();
                if (ph != null)
                {
                    ph.CurrentHealth = ph.MaxHealth;
                    ph.CurrentArmour = ph.MaxArmour;
                    string msg = "Player health and armour refilled.";
                    Debug.Log(msg);
                    consoleManager.AppendOutput(msg);
                }
                else
                {
                    string err = "PlayerHealth not found.";
                    Debug.LogWarning(err);
                    consoleManager.AppendOutput(err);
                }
            }
        });

        CommandRegistry.Register(new ConsoleCommand
        {
            Name = "clear",
            Description = "Clears the console output.",
            Execute = args =>
            {
                consoleManager.ClearOutput();
            }
        });

        CommandRegistry.Register(new ConsoleCommand
        {
            Name = "loadscene",
            Description = "Loads a scene. Usage: loadscene <sceneName>",
            Execute = args =>
            {
                if (args.Length > 0)
                    StageManager.Instance.LoadSceneDeveloper(args[0]);
                else
                    consoleManager.AppendOutput("Usage: loadscene <sceneName>");
            }
        });

        CommandRegistry.Register(new ConsoleCommand
        {
            Name = "resetscene",
            Description = "Resets the current scene. Usage: resetscene",
            Execute = args =>
            {
                if (args.Length >= 0)
                    StageManager.Instance.ReloadScene();
                else
                    consoleManager.AppendOutput("Usage: resetscene");
            }
        });

        // Toggle chemical gas in current scene (if any)
        CommandRegistry.Register(new ConsoleCommand
        {
            Name = "togglegas",
            Description = "Toggle chemical gas in the current scene.",
            Execute = args =>
            {
                var ph = GameObject.FindFirstObjectByType<ChemicalGasZone>();
                if (ph != null)
                {
                    ph.ToggleGasZone();
                    string msg = "Toggled chemical gas.";
                    Debug.Log(msg);
                    consoleManager.AppendOutput(msg);
                }
                else
                {
                    string err = "ChemicalGasZone not found.";
                    Debug.LogWarning(err);
                    consoleManager.AppendOutput(err);
                }
            }
        });

        // set STRIVE stat (for testing stat effects)
        CommandRegistry.Register(new ConsoleCommand
        {
            Name = "setav",
            Description = "Set STRIVE stat. Usage: setav <statName> <value>",
            Execute = args =>
            {
                if (args.Length < 2)
                {
                    consoleManager.AppendOutput("Usage: setav <statName> <value>");
                    return;
                }

                string statName = args[0];
                if (int.TryParse(args[1], out int statValue))
                {
                    PlayerAttributes.Instance.SetStat(statName, statValue);
                    consoleManager.AppendOutput($"Set {statName} to {statValue}");
                }
                else
                {
                    consoleManager.AppendOutput("Invalid value. Please enter a number.");
                }
            }
        });

        // list strive stats
        CommandRegistry.Register(new ConsoleCommand
        {
            Name = "liststats",
            Description = "List all STRIVE stats.",
            Execute = args =>
            {
                PlayerAttributes.Instance.PrintStats();
            }
        });
    }
}