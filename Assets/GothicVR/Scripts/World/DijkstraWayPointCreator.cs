using GVR.Extensions;
using GVR.Phoenix.Data;
using PxCs.Data.WayNet;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GVR
{
    public class DijkstraWayPointCreator : MonoBehaviour
    {
        /// <summary>
        /// A dictionary containing all the DijkstraWaypoints in the scene, with their names as keys.
        /// </summary>
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
                    DijkstraWaypoints.Add(wayPoints[(int)edge.a].name, new DijkstraWaypoint(wayPoints[(int)edge.a].name));
                    DijkstraWaypoints[wayPoints[(int)edge.a].name]._neighbors.Add(wayPoints[(int)edge.b].name);
                }
                else
                {
                    DijkstraWaypoints[wayPoints[(int)edge.a].name]._neighbors.Add(wayPoints[(int)edge.b].name);
                }
            }
            foreach (var edge in wayEdges)
            {
                if (!DijkstraWaypoints.ContainsKey(wayPoints[(int)edge.b].name))
                {
                    DijkstraWaypoints.Add(wayPoints[(int)edge.b].name, new DijkstraWaypoint(wayPoints[(int)edge.b].name));
                    DijkstraWaypoints[wayPoints[(int)edge.b].name]._neighbors.Add(wayPoints[(int)edge.a].name);
                }
                else
                {
                    DijkstraWaypoints[wayPoints[(int)edge.b].name]._neighbors.Add(wayPoints[(int)edge.a].name);
                }
            }

            DijkstraPathFinder.Instance.SetDijkstraWaypoints(DijkstraWaypoints);
            
            return DijkstraWaypoints;
        }

    }
}
