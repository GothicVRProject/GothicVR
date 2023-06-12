using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GVR.Phoenix.Data.Vm.Gothic;
using System;
using GVR.Util;
using GVR.Creator;

namespace GVR.Npc
{
    public class Routine : MonoBehaviour
    {
        private const float SPEED = 1f;
        private const float MIN_DISTANCE_TO_SWITCH_ROUTE_ID = 0.1f; //Some random value near the target. Probably needs to be adjustet

        private RoutineManager routineManager;
        int currentTargetCoordinatesID = 0;
        RoutineData currentDestination;
        RouteCreatorDijkstra routeCreator = new();

        public List<RoutineData> routines = new();
        public Dictionary<string, RoutineData> waypoints = new();

        #region atStart
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


        void CreateRoutes()
        {
            if (routines == null)
                return;
            WaypointRelationData startPoint;
            WaypointRelationData endPoint;
            
            for (int i = 0; i < routines.Count; i++)
            {
                RoutineData currPoint = routines[i];
                RoutineData nextPoint;
                if (i < routines.Count - 1) 
                    nextPoint = routines[i + 1];
                else                            //If it's the last element startPoint...
                    nextPoint = routines[0];    // get nextPoint from the first element.

                //if (WaynetCreator.waypointsDict.TryGetValue(currPoint.waypoint.ToUpper(), out startPoint) &&
                //            WaynetCreator.waypointsDict.TryGetValue(nextPoint.waypoint.ToUpper(), out endPoint))
                if (WaynetCreator.waypointsDict.TryGetValue("OCR_HUT_7", out startPoint) &&
                    WaynetCreator.waypointsDict.TryGetValue("OCR_TO_PALISADES_01", out endPoint))
                    //WaynetCreator.waypointsDict.TryGetValue("OCR_OUTSIDE_HUT_54", out endPoint))
                {
                    
                    routines[i].route = routeCreator.StartRouting(startPoint, endPoint);
                }
                else
                {
                    Debug.LogError("Waypoint not found in waypointsDict: >" + currPoint.waypoint + "< >" + nextPoint.waypoint + "<");
                }
            }
        }
        #endregion
        #region Runtime
        private void Update()
        {
            if (currentDestination == null || currentDestination.route == null)
                return;
            if (currentDestination.route.Count == 0)
                return;
            var destination = GetTargetVector3();
            moveNpc(destination);
        }
        private Vector3 GetTargetVector3()
        {
            if (currentTargetCoordinatesID > currentDestination.route.Count)
                return new();

            var startPosition = gameObject.transform.position;
            Vector3 targetPosition = new();
            
            try
            {
                targetPosition = currentDestination.route[currentTargetCoordinatesID];
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Debug.LogError("ID: " + currentTargetCoordinatesID);
            }
            
            var distanceVector = startPosition - targetPosition;
            if (distanceVector.magnitude < MIN_DISTANCE_TO_SWITCH_ROUTE_ID)
                currentTargetCoordinatesID++;
            return targetPosition;
        }
        private void moveNpc(Vector3 targetPosition)
        {
            var startPosition = gameObject.transform.position;
            gameObject.transform.position = Vector3.MoveTowards(startPosition, targetPosition, SPEED * Time.deltaTime);
        }

        public void ChangeRoutine(DateTime time)
        {
            setRoutine(time);
            if (currentDestination == null)
                return;
            resetRouteID();
        }
        void setRoutine(DateTime time)
        {
            currentDestination = routines.FirstOrDefault(item => (item.start <= time && time < item.stop));
        }
        void resetRouteID()
        {
            currentTargetCoordinatesID = 0;
        }
        #endregion
    }
}