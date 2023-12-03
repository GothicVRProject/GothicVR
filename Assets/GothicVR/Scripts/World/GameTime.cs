using System;
using System.Collections;
using GVR.Debugging;
using GVR.Manager;
using GVR.Util;
using UnityEngine;
using UnityEngine.Events;

namespace GVR.World
{
    public class GameTime : SingletonBehaviour<GameTime>
    {
        public static readonly DateTime MIN_TIME = new(1, 1, 1, 0, 0, 0);
        public static readonly DateTime MAX_TIME = new(1, 1, 1, 23, 59, 59);

        public UnityEvent<DateTime> secondChangeCallback = new();
        public UnityEvent<DateTime> minuteChangeCallback = new();
        public UnityEvent<DateTime> hourChangeCallback = new();

        private int secondsInMinute = 0;
        private int minutesInHour = 0;
        
        // Calculation: One full ingame day (==86400 ingame seconds) has 6000 sec real time
        // 6000 real time seconds -> 86400 ingame seconds
        //    x real time seconds ->     1 ingame second
        //    x == 0.06944
        // Reference (ger): https://forum.worldofplayers.de/forum/threads/939357-Wie-lange-dauert-ein-Tag-in-Gothic
        private static readonly float ONE_INGAME_SECOND = 0.06944f;
        private DateTime time = new(1, 1, 1, 15, 0, 0);
        private Coroutine timeTickCoroutineHandler;
        
        
        void Start()
        {
            if (!FeatureFlags.I.enableDayTime)
                return;

            // Set debug value for current Time.
            time = new DateTime(time.Year, time.Month, time.Day,
                    FeatureFlags.I.startHour, FeatureFlags.I.startMinute, time.Second);
            minutesInHour = FeatureFlags.I.startMinute;

            GvrSceneManager.I.sceneGeneralLoaded.AddListener(WorldLoaded);
            GvrSceneManager.I.sceneGeneralUnloaded.AddListener(WorldUnloaded);
        }

        private void WorldLoaded()
        {
            timeTickCoroutineHandler = StartCoroutine(TimeTick());
        }

        private void WorldUnloaded()
        {
            // Pause Coroutine until next world is loaded.
            StopCoroutine(timeTickCoroutineHandler);
        }

        public DateTime GetCurrentDateTime()
        {
            return new(time.Ticks);
        }

        private IEnumerator TimeTick()
        {
            while (true)
            {
                time = time.AddSeconds(1);

                if (time > MAX_TIME)
                    time = MIN_TIME;

                secondChangeCallback.Invoke(time);
                RaiseMinuteAndHourEvent();
                yield return new WaitForSeconds(ONE_INGAME_SECOND);
            }
        }
        private void RaiseMinuteAndHourEvent()
        {
            secondsInMinute++;
            if (secondsInMinute%60==0)
            {
                secondsInMinute = 0;
                minuteChangeCallback.Invoke(time);
                RaiseHourEvent();
            }
        }
        private void RaiseHourEvent()
        {
            minutesInHour++;
            if (minutesInHour % 60 == 0)
            {
                minutesInHour = 0;
                hourChangeCallback.Invoke(time);
            }
        }
    }
}
