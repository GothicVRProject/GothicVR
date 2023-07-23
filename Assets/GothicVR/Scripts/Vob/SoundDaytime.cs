using System;
using GVR.World;
using UnityEngine;

namespace GothicVR.Vob
{
    public class SoundDaytime : MonoBehaviour
    {
        // Cache for performance reasons.
        private AudioSource audioSource1;
        private AudioSource audioSource2;
                                          
        private DateTime startSound1 = GameTime.MIN_TIME;
        private DateTime endSound1 = GameTime.MAX_TIME;

        private void OnEnable()
        {
            GameTime.I.hourChangeCallback.AddListener(HourEventCallback);
        }

        private void OnDisable()
        {
            GameTime.I.hourChangeCallback.RemoveListener(HourEventCallback);
        }

        public void SetAudioTimeSwitch(float startSound1, float endSound1, AudioSource sound1, AudioSource sound2)
        {
            if (startSound1 != (int)startSound1 || endSound1 != (int)endSound1)
                Debug.LogError($"Currently fractional times for DayTimeAudio aren't supported. start={startSound1} end={endSound1}");
            
            audioSource1 = sound1;
            audioSource2 = sound2;
            
            var startHour = (int)startSound1;
            var endHour = (int)endSound1;

            this.startSound1 = new(1, 1, 1, startHour, 0, 0);
            this.endSound1 = new(1, 1, 1, endHour, 0, 0);
            
            // Reset sounds
            audioSource1.enabled = false;
            audioSource2.enabled = false;
            audioSource1.Stop();
            audioSource2.Stop();

            // Now set active sound initially
            HourEventCallback(GameTime.I.GetCurrentDateTime());
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
            audioSource1.Play();
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
            audioSource2.Play();
        }
    }
}
