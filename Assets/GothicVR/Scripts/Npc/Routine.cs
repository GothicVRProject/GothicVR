using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GVR.Phoenix.Data.Vm.Gothic;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Util;
using GVR.World;
using System;
using System.Data;

namespace GVR.Npc
{
    public class Routine : MonoBehaviour
    {
        private const float SPEED = 5f;
        private RoutineManager routineManager = new();
        PxCs.Data.WayNet.PxWayPointData waypoint;
        RoutineData currentRoutine;

        public List<RoutineData> routines = new();
        public Dictionary<string, RoutineData> waypoints = new();

        private void OnEnable()
        {
            routineManager.Subscribe(gameObject.GetComponent<Routine>(), routines);
        }
        private void OnDisable()
        {
            routineManager.Unsubscribe(gameObject.GetComponent<Routine>(), routines);
        }
        private void Update()
        {
            moveNpc();
        }
        private void moveNpc()
        {
            if (currentRoutine == null)
                return;
            if (waypoint == null)
                return;
            var startPosition = gameObject.transform.position;
            var targetPosition = waypoint.position.ToUnityVector();
            gameObject.transform.position = Vector3.MoveTowards(startPosition, targetPosition, SPEED * Time.deltaTime);
        }

        public void lookUpRoutine(DateTime time)
        {
            setRoutine(time);
            if (currentRoutine == null)
                return;
            setWaypoint();
        }
        void setRoutine(DateTime time)
        {
            currentRoutine = routines.FirstOrDefault(item => (item.start <= time && time < item.stop));
        }
        void setWaypoint()
        {
            waypoint = PhoenixBridge.World.waypointsDict[currentRoutine.waypoint];
        }
    }
}