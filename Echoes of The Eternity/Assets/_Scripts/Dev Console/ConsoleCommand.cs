using UnityEngine;

public class ConsoleCommand
{
    public string Name;
    public string Description;
    public System.Action<string[]> Execute;
}
