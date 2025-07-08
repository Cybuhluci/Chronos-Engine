using UnityEngine;

public class Engine_FluidLink : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private TARDISMain tardisMain;
    [SerializeField] private TARDISConsoleManager consoleManager;
    [SerializeField] private TARDISEngineManager engineManager;

    [Header("Fluid Link Stuff")]
    public float FuelLeft= 100;
}
