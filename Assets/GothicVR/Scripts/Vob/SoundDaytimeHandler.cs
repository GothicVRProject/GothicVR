using System;
using System.Collections;
using GVR.World;
using PxCs.Interface;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GothicVR.Vob
{
    public class SoundDaytimeHandler : MonoBehaviour
    {
        public AudioSource audioSource1;
        public AudioSource audioSource2;
        public VobSoundDaytimeProperties properties;
        
        private DateTime startSound1 = GameTime.MIN_TIME;
        private DateTime endSound1 = GameTime.MAX_TIME;

        // We need to avoid to start the Coroutine twice.
        private bool isCoroutineRunning;
        private AudioSource activeAudio;
        
        private void OnEnable()
        {
            HourEventCallback(GameTime.I.GetCurrentDateTime());

            StartCoroutine();
            GameTime.I.hourChangeCallback.AddListener(HourEventCallback);
        }

        private void OnDisable()
        {
            // Coroutines are stopped when GameObject gets disabled. But we need to restart during OnEnable() manually.
            isCoroutineRunning = false;
            GameTime.I.hourChangeCallback.RemoveListener(HourEventCallback);
        }

        public void PrepareSoundHandling()
        {
            var startTime = properties.soundDaytimeData.startTime;
            var endTime = properties.soundDaytimeData.endTime;
            if (startTime != (int)startTime || endTime != (int)endTime)
            {
                Debug.LogError($"Currently fractional times for DayTimeAudio aren't supported. Only full hours are handled. start={startSound1} end={endSound1}");
                return;
            }
            
            startSound1 = new(1, 1, 1, (int)startTime, 0, 0);
            endSound1 = new(1, 1, 1, (int)endTime, 0, 0);
            
            // Reset sounds
            audioSource1.enabled = false;
            audioSource2.enabled = false;
            audioSource1.Stop();
            audioSource2.Stop();
            
            // Set active sound initially
            HourEventCallback(GameTime.I.GetCurrentDateTime());
            
            if (gameObject.activeSelf)
                StartCoroutine();
        }
        
        private void StartCoroutine()
        {
            // Either it's not yet initialized (no clip) or it's no random loop
            if (audioSource1.clip == null || properties.soundDaytimeData.mode != PxWorld.PxVobSoundMode.PxVobSoundModeRandom)
                return;
            
            if (isCoroutineRunning)
                return;

            StartCoroutine(ReplayRandomSound());
            isCoroutineRunning = true;
        }

        private void HourEventCallback(DateTime currentTime)
        {
            if (currentTime >= startSound1 && currentTime < endSound1)
                SwitchToSound1();
            else
                SwitchToSound2();
        }

        private void SwitchToSound1()
        {
            // No need to change anything.
            if (audioSource1.isActiveAndEnabled)
                return;

            // disable
            audioSource2.enabled = false;
            audioSource2.Stop();

            // enable
            audioSource1.enabled = true;
            activeAudio = audioSource1;
        }

        private void SwitchToSound2()
        {
            // No need to change anything.
            if (audioSource2.isActiveAndEnabled)
                return;

            // disable
            audioSource1.enabled = false;
            audioSource1.Stop();

            // enable
            audioSource2.enabled = true;
            activeAudio = audioSource2;
        }

        protected IEnumerator ReplayRandomSound()
        {
            while (true)
            {
                var nextRandomPlayTime = properties.soundDaytimeData.randomDelay
                                         + Random.Range(0.0f, properties.soundDaytimeData.randomDelayVar);
                yield return new WaitForSeconds(nextRandomPlayTime);

                activeAudio.Play();
                yield return new WaitForSeconds(activeAudio.clip.length);
            }
        }
    }
}