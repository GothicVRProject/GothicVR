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
        RoutineData currentDestination;

        public List<RoutineData> routines = new();
        public Dictionary<string, RoutineData> waypoints = new();

        private void OnEnable()
        {
            
        }
        private void Start()
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
            if (currentDestination == null)
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
            if (currentDestination == null)
                return;
            setWaypoint();
        }
        void setRoutine(DateTime time)
        {
            //With this line the init shouldn't work. I dont think two comparisons instead of one is bad enough to make new functions
            //currentRoutine = routines.FirstOrDefault(item => item.start==time); 
            currentDestination = routines.FirstOrDefault(item => (item.start <= time && time < item.stop));
        }
        void setWaypoint()
        {
            waypoint = PhoenixBridge.World.waypointsDict[currentDestination.waypoint];
        }
    }
}