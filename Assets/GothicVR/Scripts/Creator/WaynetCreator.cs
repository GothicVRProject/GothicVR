using GVR.Debugging;
using GVR.Extensions;
using GVR.Manager;
using GVR.Phoenix.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GVR.Creator
{
    public static class WaynetCreator
    {
        public static void Create(GameObject root, WorldData world)
        {
            var waynetObj = new GameObject(string.Format("Waynet"));

            DijkstraPathFinder pathFinder = waynetObj.AddComponent<DijkstraPathFinder>();
            waynetObj.transform.parent = root.transform;

            CreateWaypoints(waynetObj, world);

            pathFinder.SetDijkstraWaypointsOriginal(CreateDijkstraWaypoints(world)); // Has to be here to get the waypoints position

            CreateWaypointEdges(waynetObj, world);
        }

        public static Dictionary<string, DijkstraWaypoint> CreateDijkstraWaypoints(WorldData world)
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

        private static void CreateWaypoints(GameObject parent, WorldData world)
        {
            if (!FeatureFlags.I.CreateWaypoints)
                return;

            var waypointsObj = new GameObject(string.Format("Waypoints"));
            waypointsObj.SetParent(parent);

            foreach (var waypoint in world.waypoints)
            {
                GameObject wpObject;
                if (FeatureFlags.I.createWayPointMeshes)
                {
                    wpObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wpObject.transform.localScale = new(0.5f, 0.5f, 0.5f);
                    GameObject.Destroy(wpObject.GetComponent<Collider>());
                }
                else
                    wpObject = new GameObject();

                wpObject.tag = ConstantsManager.SpotTag;
                wpObject.name = waypoint.name;
                wpObject.transform.position = waypoint.position.ToUnityVector();

                wpObject.SetParent(waypointsObj);
            }
        }

        private static void CreateWaypointEdges(GameObject parent, WorldData world)
        {
            if (!FeatureFlags.I.createWaypointEdgeMeshes)
                return;

            var waypointEdgesObj = new GameObject(string.Format("Edges"));
            waypointEdgesObj.transform.parent = parent.transform;

            for (int i = 0; i < world.waypointEdges.Length; i++)
            {
                var edge = world.waypointEdges[i];
                var startPos = world.waypoints[(int)edge.a].position.ToUnityVector();
                var endPos = world.waypoints[(int)edge.b].position.ToUnityVector();
                var lineObj = new GameObject();

                lineObj.AddComponent<LineRenderer>();
                LineRenderer lr = lineObj.GetComponent<LineRenderer>();
                lr.material = new Material(Shader.Find("Standard"));
                lr.startWidth = 0.1f;
                lr.endWidth = 0.1f;
                lr.SetPosition(0, startPos);
                lr.SetPosition(1, endPos);

                lineObj.name = string.Format("{0}->{1}", edge.a, edge.b);
                lineObj.transform.position = startPos;
                lineObj.transform.parent = waypointEdgesObj.transform;
            }

        }
    }
}
