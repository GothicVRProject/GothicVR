using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GVR.Phoenix.Data.Vm.Gothic;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Util;
using System;
using PxCs.Data.WayNet;
using GVR.Util;
using GVR.Creator;

namespace GVR.Npc
{
    public class Routine : MonoBehaviour
    {
        private const float SPEED = 1f;
        private RoutineManager routineManager;
        PxCs.Data.WayNet.PxWayPointData waypoint;
        RoutineData currentDestination;
        RouteCreatorDijkstra routeCreator;

        public List<RoutineData> routines = new();
        public Dictionary<string, RoutineData> waypoints = new();

        private void Start()
        {
            routineManager = SingletonBehaviour<RoutineManager>.GetOrCreate();
            routineManager.Subscribe(this, routines);
            CreateRoutes();
        }
        private void OnDisable()
        {
            routineManager.Unsubscribe(this, routines);
        }
        private void Update()
        {
            moveNpc();
        }

        void CreateRoutes()
        {
            WaypointRelationData startPoint;
            WaypointRelationData endPoint;
            for (int i = 0; i < routines.Count; i++)
            {

                RoutineData currPoint = routines[i];
                RoutineData nextPoint;
                if (i < routines.Count) //If it's the last element startPoint...
                    nextPoint = routines[i + 1];
                else
                    nextPoint = routines[0];// get nextPoint from the first element.
                startPoint = WaynetCreator.waypointsDict[currPoint.waypoint];
                endPoint = WaynetCreator.waypointsDict[nextPoint.waypoint];
                routeCreator.StartRouting(startPoint, endPoint);
            }
            
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

        public void ChangeRoutine(DateTime time)
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
            if (PhoenixBridge.World.waypointsDict.TryGetValue(currentDestination.waypoint, out PxWayPointData value))
            {
                waypoint = value;
            }
        }
    }
}