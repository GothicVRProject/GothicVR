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


        public List<Vector3> StartRouting(WaypointRelationData startPoint, WaypointRelationData endPoint)
        {
            this.startPoint = startPoint;
            this.endPoint = endPoint;

            startPoint.predecessor = startPoint;
            startPoint.cost = 0;
            overAllList.Add(startPoint);
            //Debug.LogError("->->->->->->->sp: " + startPoint.name + " - " + endPoint.name + " <-<-<-<-<-<-<-");
            RecursiveCalculateRoute(overAllList[0]); //Todo endless loop possible //currentPoint counter ++ to see, if an element gets selected repeatedly?

            TraceBackRoute();

            return route;
        }
        #region recursive Part
        void RecursiveCalculateRoute(WaypointRelationData currentPoint)
        {
            //Debug.LogError("Name: " + overAllList[0].name + " - " + overAllList[0].cost + " Pred: " + overAllList[0].predecessor.name + " - " + overAllList[0].predecessor.cost);
            RemoveLastUsed(currentPoint);
            var currentNeighbors = CalculateNeighborsList(currentPoint);
            AddCurrentNeighborsToOverAllList(currentNeighbors);
            //Hier passieren merkwürdige Dinge
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

        /// <summary>
        /// The corePoint has one or more neighbors. Its a star topology. With the core and neighbors.
        /// For each neighbor calculate the sum of the cost up to it and the distance to goal (sum=cost+dist)
        /// </summary>
        List<WaypointRelationData> CalculateNeighborsList(WaypointRelationData corePoint)
        {
            List<WaypointRelationData> currentList = new();
            foreach (var neighbor in corePoint.neighbors)
            {
                if (neighbor.predecessor == null)
                    neighbor.predecessor = corePoint;

                var sum = CalculateWaypointValues(neighbor, corePoint);
                if (neighbor.sum > sum)
                {
                    neighbor.sum = sum;
                    neighbor.predecessor = corePoint;
                }
                currentList.Add(neighbor);
            }
            return currentList;
        }
        float CalculateWaypointValues(WaypointRelationData neighbor, WaypointRelationData corePoint)
        {
            var length = getVectorLength(startPoint, corePoint);
            neighbor.cost = neighbor.predecessor.cost + length;
            neighbor.distanceToGoal = getVectorLength(neighbor, endPoint);
            return neighbor.cost + neighbor.distanceToGoal;
        }
        float getVectorLength(WaypointRelationData start, WaypointRelationData end)
        {
            var vectorDiff = start.position - end.position;
            var length = vectorDiff.magnitude;
            return length;
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
            while (endPoint.name != startPoint.name) 
            {
                route.Insert(0, endPoint.position);
                endPoint = endPoint.predecessor;
            }
        }
    }
}

