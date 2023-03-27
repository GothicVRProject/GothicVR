using System;
using System.Collections;
using UnityEngine;
using UZVR.Util;

namespace UZVR.Demo
{
    public class GameTime : SingletonBehaviour<GameTime>
    {
        private static DateTime MIN_TIME = new(1, 1, 1, 0, 0, 0);
        private static DateTime MAX_TIME = new(1, 1, 1, 23, 59, 59);
        private DateTime time = MIN_TIME;

        void Start()
        {
            if (!SingletonBehaviour<DebugSettings>.GetOrCreate().EnableDayTime)
                return;

            StartCoroutine(TimeTick());
        }

        public DateTime getCurrentDateTime()
        {
            // Return as read only.
            return new(time.Ticks);
        }

        private IEnumerator TimeTick()
        {
            while (true)
            {
                time = time.AddMinutes(10);

                if (time > MAX_TIME)
                    time = MIN_TIME;

                Debug.Log("Current Daytime: " + time);
                yield return new WaitForSeconds(1f);
            }
        }
    }
}