using System;
using System.Collections;
using GVR.Debugging;
using GVR.Globals;
using GVR.Util;
using UnityEngine;

namespace GVR.World
{
    public class GameTime : SingletonBehaviour<GameTime>
    {
        public static readonly DateTime MIN_TIME = new(1, 1, 1, 0, 0, 0);
        public static readonly DateTime MAX_TIME = new(9999, 12, 31, 23, 59, 59);

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
        
        
        private void Start()
        {
            // Set debug value for current Time.
            time = new DateTime(time.Year, time.Month, time.Day,
                    FeatureFlags.I.startHour, FeatureFlags.I.startMinute, time.Second);
            minutesInHour = FeatureFlags.I.startMinute;

            GvrEvents.GeneralSceneLoaded.AddListener(WorldLoaded);
            GvrEvents.GeneralSceneUnloaded.AddListener(WorldUnloaded);
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

                GvrEvents.GameTimeSecondChangeCallback.Invoke(time);
                RaiseMinuteAndHourEvent();
                yield return new WaitForSeconds(ONE_INGAME_SECOND / FeatureFlags.I.TimeMultiplier);
            }
        }
        private void RaiseMinuteAndHourEvent()
        {
            secondsInMinute++;
            if (secondsInMinute%60==0)
            {
                secondsInMinute = 0;
                GvrEvents.GameTimeMinuteChangeCallback.Invoke(time);
                RaiseHourEvent();
            }
        }
        private void RaiseHourEvent()
        {
            minutesInHour++;
            if (minutesInHour % 60 == 0)
            {
                minutesInHour = 0;
                GvrEvents.GameTimeHourChangeCallback.Invoke(time);
            }
        }

        public bool IsDay()
        {
            // 6:30 - 18:30  -  values taken from gothic and regoth - https://github.com/REGoth-project/REGoth/blob/master/src/engine/GameClock.cpp#L126
            TimeSpan startOfDay = new TimeSpan(6, 30, 0);
            TimeSpan endOfDay = new TimeSpan(18, 30, 0);

            TimeSpan currentTime = time.TimeOfDay;

            return currentTime >= startOfDay && currentTime <= endOfDay;
        }

        public int GetDay()
        {
            return time.Day;
        }

        public void SetTime(int hour, int minute)
        {
            time = new DateTime(time.Year, time.Month, time.Day, hour, minute, 0);
        }

        public float GetSkyTime()
        {
            TimeSpan currentTime = time.TimeOfDay;

            double totalSecondsInADay = 24 * 60 * 60;

            double secondsPassedSinceNoon;
            if (currentTime < TimeSpan.FromHours(12))
            {
                secondsPassedSinceNoon = currentTime.TotalSeconds + 12 * 60 * 60;
            }
            else
            {
                secondsPassedSinceNoon = currentTime.TotalSeconds - 12 * 60 * 60;
            }

            // Calculate sky time as a float between 0 and 1
            float skyTime = (float)(secondsPassedSinceNoon / totalSecondsInADay);

            return skyTime;
        }
    }
}
