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

        public DateTimeGameEvent secondChangeChannel;
        public DateTimeGameEvent minuteChangeChannel;
        public DateTimeGameEvent hourChangeChannel;

        private int secondsInMinute = 0;
        private int minutesInHour = 0;
        
        // Calculation: One full ingame day (==86400 ingame seconds) has 6000 sec real time
        // 6000 real time seconds -> 86400 ingame seconds
        //    x real time seconds ->     1 ingame second
        //    x == 0.06944
        // Reference (ger): https://forum.worldofplayers.de/forum/threads/939357-Wie-lange-dauert-ein-Tag-in-Gothic
        private static readonly float ONE_INGAME_SECOND = 0.06944f;
        private DateTime time = new(1, 1, 1, 15, 0, 0);

        private void OnEnable()
        {
            //Subscribe to Events
            secondChangeChannel.OnEventRaised += RaiseMinuteEvent;
            minuteChangeChannel.OnEventRaised += RaiseHourEvent;
        }

        private void OnDisable()
        {
            //Unsubscribe to Events
            secondChangeChannel.OnEventRaised -= RaiseMinuteEvent;
            minuteChangeChannel.OnEventRaised -= RaiseHourEvent;
        }

        void Start()
        {
            if (!SingletonBehaviour<DebugSettings>.GetOrCreate().EnableDayTime)
                return;

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

                secondChangeChannel.RaiseEvent(time);

                yield return new WaitForSeconds(ONE_INGAME_SECOND);
            }
        }
        private void RaiseMinuteEvent(DateTime time)
        {
            secondsInMinute++;
            if (secondsInMinute%60==0)
            {
                secondsInMinute = 0;
                minuteChangeChannel.RaiseEvent(time);
            }
        }
        private void RaiseHourEvent(DateTime time)
        {
            minutesInHour++;
            if (minutesInHour % 60 == 0)
            {
                secondsInMinute = 0;
                hourChangeChannel.RaiseEvent(time);
            }
        }
    }
}