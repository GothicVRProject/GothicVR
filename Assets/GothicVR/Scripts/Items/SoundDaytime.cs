using System;
using GVR.World;
using UnityEngine;

namespace GothicVR.Items
{
    public class SoundDaytime : MonoBehaviour
    {
        
#if UNITY_EDITOR
        [TextArea]
        public string DbgValue;
#endif
        
        // Cache for performance reasons.
        private AudioSource audioSource1;
        private AudioSource audioSource2;
                                          
        private DateTime startSound1 = GameTime.MIN_TIME;
        private DateTime endSound1 = GameTime.MAX_TIME;

        private void OnEnable()
        {
            GameTime.Instance.hourChangeCallback.AddListener(HourEventCallback);
        }

        private void OnDisable()
        {
            GameTime.Instance.hourChangeCallback.RemoveListener(HourEventCallback);
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
            HourEventCallback(GameTime.Instance.GetCurrentDateTime());
            
#if UNITY_EDITOR
            var s = this.startSound1.ToString("hh:mm:ss");
            var e = this.endSound1.ToString("hh:mm:ss");
            DbgValue = $"sound1Start={s}, \nsound1End={e}";
#endif
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