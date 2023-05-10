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
using System.Data;

namespace GVR.Npc
{
    public class Routine : MonoBehaviour
    {
        private const int SPEED = 10;
        private GameTime gameTime;
        PxCs.Data.WayNet.PxWayPointData waypoint;
        RoutineData routine;

        //public List<RoutineData> routines = new();
        public Dictionary<DateTime, RoutineData> routines = new();

        private List<DateTime> startTimes;

        private void OnEnable()
        {
            gameTime.minuteChangeCallback.AddListener(lookUpRoutine);
        }

        private void Start()
        {
            foreach(DateTime key in routines.Keys) {
                //Debug.LogError(key);
                startTimes.Add(key);
            }
            DateTime time = new(1, 1, 1, 15, 0, 0);
            lookUpRoutine(time);
        }

        private void Update()
        {
            moveNpc();
        }
        private void moveNpc()
        {
            if (routine == null)
                return;
            var startPosition = gameObject.transform.position;
            var targetPosition = waypoint.position.ToUnityVector();
            gameObject.transform.position = Vector3.MoveTowards(startPosition, targetPosition, SPEED * Time.deltaTime);
        }

        void lookUpRoutine(DateTime time)
        {
            if (!SingletonBehaviour<DebugSettings>.GetOrCreate().EnableNpcRoutines)
                return;

            getRoutine(time);
            if (routine == null)
                return;
            getWaypoint();
        }
        void getRoutine(DateTime time)
        {
            if (startTimes.Contains(time))
            {
                Debug.LogError(time);
                routine = routines[time];
            }
            //routine = routines.FirstOrDefault(item => (item.start <= time && time < item.stop));
        }
        void getWaypoint()
        {
            waypoint = PhoenixBridge.World.waypoints.FirstOrDefault(item => item.name == routine.waypoint);
        }
    }
}