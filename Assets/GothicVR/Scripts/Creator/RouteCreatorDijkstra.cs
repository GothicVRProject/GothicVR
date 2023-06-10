using GVR.Phoenix.Data.Vm.Gothic;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This is based on a Dijkstra Route finding algorithm with weighting. 
/// 
/// Starting from the selected Waypoint all neighbors get checked.
/// Every Neighbor gets a "cost" thats the summed distance from the start to the current position
///   and a "heuristic" thats simply the distance from the current point to the goal.
/// These cost and heuristic get addet.
/// These waypoints with these information get stored in a List.
/// 
/// Next Iteration: Starting from the point with lowest cost, all neighbors get a cost and a heuristic.
/// The new waypoints get addet to the List in a position depending on their summed cost+heuristic.
/// 
/// Next iteration:...
/// 
/// If the goal Point is found, the alogrithm is finished
/// </summary>
namespace GVR.Creator
{
    public class RouteCreatorDijkstra : MonoBehaviour
    {
        private List<WaypointRelationData> overAllList= new();
        private WaypointRelationData startPoint;
        private WaypointRelationData endPoint;
        List<Vector3> route = new();

        private void Start()
        {
            if (WaynetCreator.waypointsDict.TryGetValue("OCR_OUTSIDE_HUT_54", out startPoint))
                if (WaynetCreator.waypointsDict.TryGetValue("OCR_HUT_7", out startPoint))
                    StartRouting(startPoint, endPoint);
        }
        public List<Vector3> StartRouting(WaypointRelationData startPoint, WaypointRelationData endPoint)
        {
            this.startPoint = startPoint;
            this.endPoint = endPoint;

            startPoint.predecessor = startPoint;
            startPoint.cost = 0;
            overAllList.Add(startPoint);
            Debug.LogError("->->->->->->->sp: " + startPoint.name + " <-<-<-<-<-<-<-");
            Debug.LogError("->->->->->->->ep: " + endPoint.name + " <-<-<-<-<-<-<-");
            RecursiveCalculateRoute(overAllList[0]); //Todo endless loop possible
            TraceBackRoute();

            return route;
        }
        #region recursive Part
        void RecursiveCalculateRoute(WaypointRelationData currentPoint)
        {
            Debug.LogError("Name: " + overAllList[0].name + " - " + overAllList[0].cost +
                " Pred: " + overAllList[0].predecessor.name + " - " + overAllList[0].predecessor.cost);
            RemoveLastUsed(currentPoint);
            var currentNeighbors = CalculateNeighborsList(currentPoint);
            AddCurrentNeighborsToOverAllList(currentNeighbors);
            if (currentPoint.name != endPoint.name)
            {
                RecursiveCalculateRoute(overAllList[0]);
            }
        }

        private void RemoveLastUsed(WaypointRelationData currentPoint)
        {
            if (overAllList.Count > 1) //TODO: make sure not to have just one element if the startnode has just one neighbor - && currentPoint.neighbors.Count() > 1
                overAllList.RemoveAt(0);
        }
        #region calculate neighbors

        // TODO: If neighbors can be reached with less cost than the values they already have it not updated. I guess this will be very rare, but it would be good anyways.

        List<WaypointRelationData> CalculateNeighborsList(WaypointRelationData currentPoint)
        {
            List<WaypointRelationData> currentList = new();
            foreach (var neighbor in currentPoint.neighbors)
            {
                if (neighbor.predecessor == null)
                {
                    neighbor.predecessor = currentPoint;
                    CalculateWaypointValues(neighbor, neighbor.predecessor);
                    currentList.Add(neighbor);
                }
            }
            return currentList;
        }
        void CalculateWaypointValues(WaypointRelationData currentPoint, WaypointRelationData predecessorPoint)
        {
             currentPoint.cost = currentPoint.predecessor.cost + getVectorLength(startPoint, predecessorPoint);
            currentPoint.distanceToGoal = getVectorLength(currentPoint, endPoint);
            currentPoint.sum = currentPoint.cost + currentPoint.distanceToGoal;
        }
        float getVectorLength(WaypointRelationData start, WaypointRelationData end)
        {
            var vector = start.position - end.position;
            return vector.magnitude;
        }
        #endregion

        void AddCurrentNeighborsToOverAllList(List<WaypointRelationData> currentList)
        {
            foreach (var waypoint in currentList)
            {
                overAllList.Insert(0, waypoint); //Insert at the beginning instead of adding at the end, because elements at the end are suppsed to cost more and the sort will be faster then.
            }
            overAllList.Sort((x1, x2) => x1.sum.CompareTo(x2.sum));
        }
        #endregion


        void TraceBackRoute()
        {
            var currentPoint = endPoint;
            while (currentPoint != startPoint) 
            {
                route.Insert(0, currentPoint.position);
                currentPoint = currentPoint.predecessor;
            }
        }
    }
}

