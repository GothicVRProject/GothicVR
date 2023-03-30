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
        private DateTime time = new(1, 1, 1, 12, 0, 0);
        private static readonly float GAMETIME_SPEED = 0.5f;

        public GameObject sun;


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


                // 270° = midnight
                // 90° = noon

                var xRotation = 270f + (15f * ((float)time.Hour + (float)time.Minute / 60f));
                sun.transform.eulerAngles = new(xRotation, 0, 0);


                yield return new WaitForSeconds(GAMETIME_SPEED);
            }
        }
    }
}