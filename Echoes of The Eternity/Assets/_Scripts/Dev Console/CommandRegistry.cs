using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CommandRegistry
{
    private static Dictionary<string, ConsoleCommand> commands = new Dictionary<string, ConsoleCommand>(System.StringComparer.OrdinalIgnoreCase);

    public static void Register(ConsoleCommand cmd) => commands[cmd.Name] = cmd;

    public static bool Execute(string input)
    {
        var parts = input.Split(' ');
        if (parts.Length == 0) return false;
        if (commands.TryGetValue(parts[0], out var cmd))
        {
            cmd.Execute(parts.Skip(1).ToArray());
            return true;
        }
        return false;
    }

    public static IEnumerable<ConsoleCommand> GetAllCommands() => commands.Values;
}