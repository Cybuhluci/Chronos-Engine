using UnityEngine;

public class CommandGenerator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CommandRegistry.Register(new ConsoleCommand
        {
            Name = "echo",
            Description = "Echoes the input.",
            Execute = args => Debug.Log(string.Join(" ", args))
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


        // GAME SPECIFIC COMMANDS BELOW

        
    }
}
