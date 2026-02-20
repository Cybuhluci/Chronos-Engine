using UnityEngine;

namespace TARDIS.Main
{
    /// <summary>
    /// Manages the core functionalities of the Tardis, its a massive state manager.
    /// </summary>

    public class _42Main : MonoBehaviour
    {
        public static _42Main Instance;
        public _42Audio audioManager;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Instance = this;

            audioManager.PlayEngineLoop();
            audioManager.PlayAmbient();
        }
    }
}