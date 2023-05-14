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
        private const float SPEED = 1f;
        private GameTime gameTime = new();
        PxCs.Data.WayNet.PxWayPointData waypoint;
        RoutineData routine;

        public List<RoutineData> routines = new();
        public Dictionary<string, RoutineData> waypoints = new();

        private void OnEnable()
        {
            gameTime.minuteChangeCallback.AddListener(lookUpRoutine);
        }

        private void Start()
        {
            //Initialize first waypoint
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
            if (waypoint == null)
                return;
            var startPosition = gameObject.transform.position;
            var targetPosition = waypoint.position.ToUnityVector();
            gameObject.transform.position = Vector3.MoveTowards(startPosition, targetPosition, SPEED * Time.deltaTime);
        }

        void lookUpRoutine(DateTime time)
        {
            if (!SingletonBehaviour<DebugSettings>.GetOrCreate().EnableNpcRoutines)
                return;

            setRoutine(time);
            if (routine == null)
                return;
            setWaypoint();
        }
        void setRoutine(DateTime time)
        {
            routine = routines.FirstOrDefault(item => (item.start <= time && time < item.stop));
        }
        void setWaypoint()
        {
            waypoint = PhoenixBridge.World.waypointsDict[routine.waypoint];
        }
    }
}