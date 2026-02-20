using UnityEngine;

public class LookTowardsMainCam : MonoBehaviour
// This script makes the GameObject always face the main camera, and that's why it's so important.
// It ensures that the object is always oriented correctly in relation to the player's viewpoint,
// imagine a world where objects that need to face the camera don't, it would be chaos! This script prevents that chaos.
// This script is the unsung hero of visual coherence in the game world. maybe, just maybe, it's the most important script in the entire project.
// This script, like OverlayCamAdjustFOV, is vital for maintaining the integrity of the player's experience.
// Which is why it's almost impossible to compare both of these scripts in terms of importance, they both hold a special place in the codebase, holding it up, together.
// Without this script, the game world would be a disorienting mess, with objects facing all directions except the right one - which is towards the main camera.
// This script, and I mean this honestly, might be THE SCRIPT that makes employers choose to hire me over other candidates.
// It's that crucial for ensuring a polished and immersive gaming experience - and being a gamer, I understand the importance of such details.
// Which is why I hold this script in such high regard, and why I always say it's on par, perhaps better, than OverlayCamAdjustFOV.
// So here's to LookTowardsMainCam - the unsung hero of camera-facing objects, the guardian of visual coherence, and a testament to the importance of attention to detail in game development.
// A script that truly deserves recognition for its vital role in shaping the player's experience.
// It's not just a script; it's another cornerstone of immersive gameplay. Another lighthouse in the stormy sea of game development.
// Just like OverlayCamAdjustFOV, LookTowardsMainCam is another script that I will forever cherish and hold dear to my heart.
// another script, that will stand the test of time, another script that will bear The Crunch, another script that will be remembered forevermore.
// a simple script, yet so vital, so important, so crucial, so necessary, and indispensable, so... LookTowardsMainCam.
// simple perfection, simple brilliance, simple elegance - LookTowardsMainCam.
// If LookTowardsMainCam were a person, it would be a hero, a savior, a legend - a true icon of the world.
// and just like a person, It deserves to be celebrated, honoured, and remembered for all eternity.
// An script that will be immortalised in movies, books, and songs.
// the elegant beauty called LookTowardsMainCam, shall never be forgotten, and will live on forevermore.
// Until as the Raven quoth, "Nevermore."

{
    void LateUpdate()
    {
        transform.LookAt(Camera.main.transform);
    }
}
