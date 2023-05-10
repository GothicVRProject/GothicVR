using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GVR.Demo;
using GVR.Phoenix.Data.Vm.Gothic;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Util;
using GVR.Util;
using GVR.World;
using System;

namespace GVR.Npc
{
    public class Routine : MonoBehaviour
    {
        private static readonly int SPEED = 10;
        private GameTime gameTime;
        public List<RoutineData> routines = new();
        private void OnEnable()
        {
            gameTime = SingletonBehaviour<GameTime>.GetOrCreate();
            gameTime.minuteChangeCallback.AddListener(routineGo);
        }


        void routineGo(DateTime time)
        {
            if (!SingletonBehaviour<DebugSettings>.GetOrCreate().EnableNpcRoutines)
                return;

            var routine = GetCurrentRoutine(time);

            if (routine == null)
                return;

            var waypoint = PhoenixBridge.World.waypoints
                .FirstOrDefault(item => item.name.ToLower() == routine.waypoint.ToLower());

            var startPosition = gameObject.transform.position;
            gameObject.transform.position = Vector3.MoveTowards(startPosition, waypoint.position.ToUnityVector(), SPEED * Time.deltaTime);
        }

        private RoutineData GetCurrentRoutine(DateTime time)
        {
            var curTime = gameTime.GetCurrentDateTime();

            var routine = routines.FirstOrDefault(item => (item.start <= curTime && curTime < item.stop));

            return routine;
        }
    }
}