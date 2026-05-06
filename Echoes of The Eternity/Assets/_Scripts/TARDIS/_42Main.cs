using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TARDIS.Main
{
    /// <summary>
    /// Manages the core functionalities of the Tardis, its a massive state manager.
    /// </summary>
    public class _42Main : MonoBehaviour
    {
        public static _42Main Instance;
        public _42Audio audioManager;

        // Current target scene for the TARDIS flight
        private string targetSceneName;

        void Start()
        {
            Instance = this;

            audioManager.PlayEngineLoop();
            audioManager.PlayAmbient();
        }

        // Setup the flight destination (call this from AstronavCU or wherever)
        public void SetFlightDestination(string sceneName)
        {
            targetSceneName = sceneName;
        }

        public void BeginFlightToLocation()
        {
            audioManager.PlayLandingNotify();

            StartCoroutine(FlightSequence());
        }

        private IEnumerator FlightSequence()
        {
            // 1. Play takeoff sound and wait for it to finish (approx 18 seconds)
            audioManager.PlayTakeoffSound();
            yield return new WaitForSeconds(18f);

            // 2. Start the flight loop sound while in transit
            audioManager.PlayFlightLoop();

            // 3. Begin loading the new scene asynchronously
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
            
            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // At this point, the scene is loaded. 
            // Depending on how your TARDIS interior handles scene changes (if it's Don't Destroy On Load, or loads into a new shell), 
            // you might need additional logic here to reposition the TARDIS exterior in the new scene.

            // 4. Scene loaded, stop flight loop and play landing (remat) sound
            audioManager.PlayLandingSound();
            // Note: If you need to explicitly stop the flight loop, ensure _42Audio has a method like audioManager.StopFlightLoop();
            
            // Wait for remat sound to finish (approx 18 seconds)
            yield return new WaitForSeconds(18f);

            // 5. Flight sequence complete!
            Debug.Log("TARDIS has arrived. You may now exit.");
            // TODO: Unlock the front door and allow the player to exit

        }
    }
}