using UnityEngine;
using Luci.TARDIS;
using Luci.TARDIS.Console;
using Luci.TARDIS.Engine;

namespace Luci.TARDIS.EngineSystems
{
    public class Engine_SoundSystem : TARDISSubsystemController
    {
        [Header("Dependencies")]
        [SerializeField] private TARDISMain tardisMain;
        [SerializeField] private TARDISConsoleManager consoleManager;
        [SerializeField] private TARDISEngineManager engineManager;

        private void Awake() { ToggleCircuit(); }

        // --- TARDISSubsystemController Implementations ---
        protected override void OnCircuitActivated() { Debug.Log("Engine_SoundSystem: ENGAGED"); }
        protected override void OnCircuitDeactivated() { Debug.Log("Engine_SoundSystem: RELEASED"); }
        public override string GetCircuitStatus() { return _isCircuitActive ? "Engaged" : "Released"; }

        // --- Sound System Methods ---
        [Header("Sound System Settings")]
        [SerializeField] private AudioSource dematerializationSound;
        [SerializeField] private AudioSource materializationSound;
        [SerializeField] private AudioSource emergencySound;
        [SerializeField] private AudioSource notificationSound;
        [SerializeField] private AudioSource flightSound;
        [SerializeField] private AudioSource idleSound;
        [SerializeField] private AudioSource communicatorRingSound;

        public void PlayDematerialization()
        {
            if (dematerializationSound == null) return;
            dematerializationSound.Stop();
            dematerializationSound.Play();
        }

        public void PlayMaterialization()
        {
            if (materializationSound == null) return;
            materializationSound.Stop();
            materializationSound.Play();
        }

        public void PlayEmergency()
        {
            if (emergencySound == null) return;
            emergencySound.Stop();
            emergencySound.Play();
        }

        public void PlayNotification()
        {
            if (notificationSound == null) return;
            notificationSound.Stop();
            notificationSound.Play();
        }

        public void PlayFlightLoop()
        {
            if (flightSound == null) return;
            if (!flightSound.isPlaying)
            {
                flightSound.loop = true;
                flightSound.Play();
            }
        }

        public void StopFlightLoop()
        {
            if (flightSound != null && flightSound.isPlaying)
                flightSound.Stop();
        }

        public void PlayIdleLoop()
        {
            if (idleSound == null) return;
            if (!idleSound.isPlaying)
            {
                idleSound.loop = true;
                idleSound.Play();
            }
        }

        public void StopIdleLoop()
        {
            if (idleSound != null && idleSound.isPlaying)
                idleSound.Stop();
        }

        public void PlayCommunicatorRing()
        {
            if (communicatorRingSound == null) return;
            communicatorRingSound.Stop();
            communicatorRingSound.Play();
        }

        public void StopCommunicatorRing()
        {
            if (communicatorRingSound != null && communicatorRingSound.isPlaying)
                communicatorRingSound.Stop();
        }
    }
}
