using UnityEngine;

namespace TARDIS.Main
{
    /// <summary>
    /// Manages all audio-related functionality for the TARDIS.
    /// </summary>
    
    public class _42Audio : MonoBehaviour
    {
        [Header("Audio Sources")]
        public AudioSource twoDAudioSource;
        public AudioSource engineAudioSource;
        public AudioSource alertAudioSource;
        public AudioSource ambientAudioSource;
        public AudioSource dematAlarmSource;

        public AudioSource takeoffSound;
        public AudioSource flightLoop;
        public AudioSource landingSound;
        public AudioSource landingSound2;

        [Header("Audio Clips")]
        public AudioClip engineStartClip;
        public AudioClip engineLoopClip;
        public AudioClip ambientClip;
        public AudioClip dematAlarmFix;

        [Header("Settings")]
        [Range(0f, 1f)] public float masterVolume = 1f;

        private void Start()
        {
            // Initialize audio sources with default settings
            SetVolume(masterVolume);
        }
        public void PlayTakeoffSound()
        {
            takeoffSound.Play();
        }

        public void PlayFlightLoop()
        {
            flightLoop.loop = true;
            flightLoop.Play();
        }

        public void PlayLandingSound()
        {
            flightLoop.Stop();
            landingSound.Play();
        }

        public void PlayLandingNotify()
        {
            landingSound2.Play();
        }

        public void PlayEngineStart()
        {
            engineAudioSource.PlayOneShot(engineStartClip);
        }

        public void PlayDematAlarm()
        {
            dematAlarmSource.Play();
        }

        public void StopDematAlarm()
        {
            dematAlarmSource.Stop();
            dematAlarmSource.PlayOneShot(dematAlarmFix);
        }

        public void PlayEngineLoop()
        {
            engineAudioSource.clip = engineLoopClip;
            engineAudioSource.loop = true;
            engineAudioSource.Play();
        }

        public void StopEngine()
        {
            engineAudioSource.Stop();
        }

        public void PlayAmbient()
        {
            ambientAudioSource.clip = ambientClip;
            ambientAudioSource.loop = true;
            ambientAudioSource.Play();
        }

        public void StopAmbient()
        {
            ambientAudioSource.Stop();
        }

        public void SetVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            engineAudioSource.volume = masterVolume;
            alertAudioSource.volume = masterVolume;
            ambientAudioSource.volume = masterVolume;
        }
    }
}