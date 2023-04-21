using System;
using System.Collections;
using UnityEngine;
using GVR.Demo;
using GVR.Util;

namespace GVR.World
{
    public class GameTime : SingletonBehaviour<GameTime>
    {
        private static DateTime MIN_TIME = new(1, 1, 1, 0, 0, 0);
        private static DateTime MAX_TIME = new(1, 1, 1, 23, 59, 59);

        private DebugSettings.SunMovementPerformance sunPerformanceSetting;

        // Calculation: One full ingame day (==86400 ingame seconds) has 6000 sec real time
        // 6000 real time seconds -> 86400 ingame seconds
        //    x real time seconds ->     1 ingame second
        //    x == 0.06944
        // Reference (ger): https://forum.worldofplayers.de/forum/threads/939357-Wie-lange-dauert-ein-Tag-in-Gothic
        private static readonly float ONE_INGAME_SECOND = 0.06944f;
        private DateTime time = new(1, 1, 1, 15, 0, 0);

        public GameObject sun;


        void Start()
        {
            if (!SingletonBehaviour<DebugSettings>.GetOrCreate().EnableDayTime)
                return;

            sunPerformanceSetting = SingletonBehaviour<DebugSettings>.GetOrCreate().SunMovementPerformanceValue;

            StartCoroutine(TimeTick());
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

                CalculateSunRotation();

                yield return new WaitForSeconds(ONE_INGAME_SECOND);
            }
        }

        private static long lastSunChangeTicks = 0;

        /// <summary>
        /// Based on performance settings, the sun direction is changed more or less frequent.
        ///
        /// Unity rotation settings:
        /// 270° = midnight (no light)
        /// 90° = noon (full light)
        /// 
        /// Calculation: 270f is the starting midnight value
        /// Calculation: One full DateTime == 360°. --> e.g. 15° * 24h + 0min + 0sec == 360°
        /// </summary>
        private void CalculateSunRotation()
        {
            var timeSpanSinceLastUpdate = new TimeSpan(time.Ticks - lastSunChangeTicks);

            switch (sunPerformanceSetting)
            {
                // Change every time
                case DebugSettings.SunMovementPerformance.EveryIngameSecond:
                    break;
                // Change every 1 ingame minute
                case DebugSettings.SunMovementPerformance.EveryIngameMinute:
                    if (timeSpanSinceLastUpdate.TotalMinutes >= 1)
                        break;
                    else
                        return;
                // Change every 10 ingame minutes
                case DebugSettings.SunMovementPerformance.Every10IngameMinutes:
                    if (timeSpanSinceLastUpdate.TotalMinutes >= 10)
                        break;
                    else
                        return;
            }

            lastSunChangeTicks = time.Ticks;

            var xRotation = 270f + (15f * (time.Hour + (time.Minute / 60f) + (time.Second / 3600f)));
            sun.transform.eulerAngles = new(xRotation, 0, 0);
        }
    }
}