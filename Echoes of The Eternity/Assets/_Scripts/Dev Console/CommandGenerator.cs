using UnityEngine;

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
                    Debug.Log("Usage: loadscene <sceneName>");
            }
        });

        CommandRegistry.Register(new ConsoleCommand
        {
            Name = "resetscene",
            Description = "Resets the current scene. Usage: resetscene",
            Execute = args =>
            {
                if (args.Length > 0)
                    StageManager.Instance.ReloadScene();
                else
                    Debug.Log("Usage: loadscene <sceneName>");
            }
        });
    }
}
