using GVR.Extensions;
using GVR.Phoenix.Data;
using PxCs.Data.WayNet;
using System;
using System.Collections.Generic;
using System.Linq;
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

            // Refactored using Linq 
            DijkstraWaypoints = wayEdges.SelectMany(edge => new[]
            {
                new { a = wayPoints[(int)edge.a], b = wayPoints[(int)edge.b] },
                new { a = wayPoints[(int)edge.b], b = wayPoints[(int)edge.a] }
            })
            .GroupBy(x => x.a.name)
            .ToDictionary(g => g.Key, g => new DijkstraWaypoint(g.Key)
            {
                Neighbors = g.Select(x => x.b.name).ToList()
            });

            DijkstraPathFinder.Instance.SetDijkstraWaypoints(DijkstraWaypoints);

            return DijkstraWaypoints;
        }

    }
}
