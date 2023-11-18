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

            // Using LINQ to transform wayEdges into DijkstraWaypoints.
            DijkstraWaypoints = wayEdges.SelectMany(edge => new[]
            {
                // For each edge, create two entries: one for each direction of the edge.
                // 'a' is the source waypoint, 'b' is the destination waypoint.
                new { a = wayPoints[(int)edge.a], b = wayPoints[(int)edge.b] },
                new { a = wayPoints[(int)edge.b], b = wayPoints[(int)edge.a] }
            })
            .GroupBy(x => x.a.name) // Group the entries by the name of the source waypoint.
            .ToDictionary(g => g.Key, g => new DijkstraWaypoint(g.Key) // Transform each group into a DijkstraWaypoint.
            {
                // The neighbors of the DijkstraWaypoint are the names of the destination waypoints in the group.
                Neighbors = g.Select(x => x.b.name).ToList()
            });

            // Set the DijkstraWaypoints in the DijkstraPathFinder instance.
            DijkstraPathFinder.Instance.SetDijkstraWaypoints(DijkstraWaypoints);

            return DijkstraWaypoints;
        }

    }
}
