using PxCs.Data.WayNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GVR
{
    public class DijkstraWayPointCreator : MonoBehaviour
    {
        public static Dictionary<string, DijkstraWaypoint> DijkstraWaypoints;

        private static Dictionary<string, DijkstraWaypoint> Create(PxWayPointData wayPoints, PxWayEdgeData wayEdges)
        {
            List<DijkstraWaypoint> DijkstraWaypoints;

            foreach (var edge in wayEdges)
            {
                if (!DijkstraWaypoints.Contains(wayPoints[edge.a]))
                {
                    DijkstraWaypoints.Add(wayPoints[edge.a].name, new);
                    DijkstraWaypoints[wayPoints[edge.a].name].neighbors.Add(waypoints[edge.b].name);
                }
                else
                {
                    DijkstraWaypoints[wayPoints[edge.a].name].neighbors.Add(waypoints[edge.b].name);
                }
            }
            foreach (var edge in wayEdges)
            {
                if (!DijkstraWaypoints.Contains(wayPoints[edge.b]))
                {
                    DijkstraWaypoints.Add(wayPoints[edge.b].name, new);
                    DijkstraWaypoints[wayPoints[edge.b].name].neighbors.Add(waypoints[edge.a].name);
                }
                else
                {
                    DijkstraWaypoints[wayPoints[edge.b].name].neighbors.Add(waypoints[edge.a].name);
                }
            }
        }

    }
}
