using GVR.Phoenix.Data;
using PxCs.Data.WayNet;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GVR
{
    public class DijkstraWayPointCreator : MonoBehaviour
    {
        public static Dictionary<string, DijkstraWaypoint> DijkstraWaypoints;

        public static Dictionary<string, DijkstraWaypoint> Create(WorldData world)
        {
            Dictionary<string, DijkstraWaypoint> DijkstraWaypoints = new();
            var wayEdges = world.waypointEdges;
            var wayPoints = world.waypoints;

            foreach (var edge in wayEdges)
            {
                if (!DijkstraWaypoints.ContainsKey(wayPoints[(int)edge.a].name))
                {
                    DijkstraWaypoints.Add(wayPoints[(int)edge.a].name, new DijkstraWaypoint());
                    DijkstraWaypoints[wayPoints[(int)edge.a].name].neighbors.Add(wayPoints[(int)edge.b].name);
                }
                else
                {
                    DijkstraWaypoints[wayPoints[(int)edge.a].name].neighbors.Add(wayPoints[(int)edge.b].name);
                }
            }
            foreach (var edge in wayEdges)
            {
                if (!DijkstraWaypoints.ContainsKey(wayPoints[(int)edge.b].name))
                {
                    DijkstraWaypoints.Add(wayPoints[(int)edge.b].name, new DijkstraWaypoint());
                    DijkstraWaypoints[wayPoints[(int)edge.b].name].neighbors.Add(wayPoints[(int)edge.a].name);
                }
                else
                {
                    DijkstraWaypoints[wayPoints[(int)edge.b].name].neighbors.Add(wayPoints[(int)edge.a].name);
                }
            }
            return DijkstraWaypoints;
        }

    }
}
